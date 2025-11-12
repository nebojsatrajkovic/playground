using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Core.Shared.Utils.ThreadsafeCollections
{
    /// <summary>
    /// Thread-safe lightweight dictionary optimized for frequent reads and occasional writes.
    /// Optionally supports automatic expiration based on monotonic Stopwatch ticks,
    /// avoiding DateTime clock drift. Suitable for in-memory caches or session tracking
    /// with moderate concurrency requirements.
    /// </summary>
    public class CORE_TS_SlimDictionary<TKey, TValue> : IDisposable where TKey : notnull
    {
        private volatile bool isDisposing;

        private readonly ReaderWriterLockSlim _lock;

        private Dictionary<TKey, (TValue Value, long LastAccessedTicks)> collection;

        public CORE_TS_SlimDictionary(int initialCapacity = 0)
        {
            isDisposing = false;
            _lock = new();
            collection = initialCapacity > 0 ? new(initialCapacity) : new();
        }

        private readonly TimeSpan? expirationTime;
        private readonly CancellationTokenSource? cts;
        private readonly Task? cleanupTask;
        private readonly long? expirationTicks;

        public CORE_TS_SlimDictionary(TimeSpan expiration, int initialCapacity = 0)
        {
            isDisposing = false;
            _lock = new();
            collection = initialCapacity > 0 ? new(initialCapacity) : new();

            expirationTime = expiration;
            cts = new CancellationTokenSource();
            cleanupTask = Task.Run(CleanupExpiredEntries, cts.Token);
            expirationTicks = (long)(expiration.TotalSeconds * Stopwatch.Frequency);
        }

        /// <summary>
        /// Gets the number of key/value pairs contained in the thread-safe collection.
        /// </summary>
        public int Count
        {
            get
            {
                ThrowIfDisposed();

                _lock.EnterReadLock();

                try
                {
                    return collection.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Deletes a value from the thread-safe collection if the key is present. If key is not found result is false.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool TryDeleteValue(TKey key)
        {
            ThrowIfDisposed();

            _lock.EnterWriteLock();

            try
            {
                return collection.Remove(key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Adds or updates the value for the specified key. If the value exists it will be overriden.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void TryAddOrUpdateValue(TKey key, TValue value)
        {
            ThrowIfDisposed();

            _lock.EnterWriteLock();

            try
            {
                collection[key] = (value, Stopwatch.GetTimestamp());
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool TryGetValue(TKey key, out TValue? value)
        {
            ThrowIfDisposed();

            _lock.EnterUpgradeableReadLock();

            try
            {
                if (collection.TryGetValue(key, out var entry))
                {
                    value = entry.Value;
                    _lock.EnterWriteLock();
                    try
                    {
                        collection[key] = (entry.Value, Stopwatch.GetTimestamp());
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                    return true;
                }

                value = default;
                return false;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public void Remove(Func<TValue, bool> ShouldRemove)
        {
            ThrowIfDisposed();

            var keysToRemove = new List<TKey>();

            _lock.EnterReadLock();

            try
            {
                foreach (var kvp in collection)
                {
                    if (ShouldRemove(kvp.Value.Value))
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }

            if (keysToRemove.Count == 0) return;

            _lock.EnterWriteLock();

            try
            {
                foreach (var key in keysToRemove)
                {
                    collection.Remove(key);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Dispose()
        {
            if (isDisposing) return;
            isDisposing = true;
            GC.SuppressFinalize(this);

            try
            {
                cts?.Cancel();
                cleanupTask?.Wait(TimeSpan.FromSeconds(2));
            }
            catch (Exception)
            {

            }
            finally
            {
                if (_lock.TryEnterWriteLock(500))
                {
                    try
                    {
                        collection.Clear();
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                        _lock.Dispose();
                    }
                }
                else
                {
                    _lock.Dispose();
                }
            }
        }

        async Task CleanupExpiredEntries()
        {
            if (expirationTicks is null) return;

            var delay = TimeSpan.FromMilliseconds(Math.Max(1000, expirationTime!.Value.TotalMilliseconds / 2));

            while (cts != null && !cts.Token.IsCancellationRequested)
            {
                await Task.Delay(delay, cts.Token).ConfigureAwait(false);

                ThrowIfDisposed();

                _lock.EnterWriteLock();

                try
                {
                    long now = Stopwatch.GetTimestamp();

                    var expiredKeys = new List<TKey>();

                    foreach (var kvp in collection)
                    {
                        if ((now - kvp.Value.LastAccessedTicks) > expirationTicks)
                        {
                            expiredKeys.Add(kvp.Key);
                        }
                    }

                    foreach (var key in expiredKeys)
                    {
                        collection.Remove(key);
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(isDisposing, this);
    }
}