using System.Runtime.CompilerServices;

namespace Core.Shared.Utils.ThreadsafeCollections
{
    public class CORE_TS_Dictionary<TKey, TValue> : IDisposable where TKey : notnull
    {
        Dictionary<TKey, (TValue Value, DateTime LastAccessed)> dictionary = [];
        readonly object padlock = new();
        bool disposed = false;

        readonly TimeSpan? expirationTime;
        readonly CancellationTokenSource? cts;
        readonly Task? cleanupTask;

        public CORE_TS_Dictionary()
        {

        }

        public CORE_TS_Dictionary(TimeSpan expiration)
        {
            expirationTime = expiration;
            cts = new CancellationTokenSource();
            cleanupTask = Task.Run(CleanupExpiredEntries, cts.Token);
        }

        /// <summary>
        /// Get the count of elements in a thread-safe manner.
        /// </summary>
        public int Count
        {
            get
            {
                ThrowIfDisposed();

                lock (padlock)
                {
                    return dictionary.Count;
                }
            }
        }

        /// <summary>
        /// Add a key-value pair in a thread-safe manner
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            ThrowIfDisposed();

            lock (padlock)
            {
                dictionary[key] = (value, DateTime.UtcNow);
            }
        }

        /// <summary>
        /// Try to add a key-value pair, returning false if the key already exists
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryAdd(TKey key, TValue value)
        {
            ThrowIfDisposed();

            lock (padlock)
            {
                if (!dictionary.ContainsKey(key))
                {
                    dictionary[key] = (value, DateTime.UtcNow);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Adds or updates a key-value pair in a thread-safe manner
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddOrUpdate(TKey key, TValue value)
        {
            ThrowIfDisposed();

            lock (padlock)
            {
                dictionary[key] = (value, DateTime.UtcNow);
            }
        }

        /// <summary>
        /// Get the value associated with the given key in a thread-safe manner
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue? Get(TKey key)
        {
            ThrowIfDisposed();

            lock (padlock)
            {
                if (dictionary.TryGetValue(key, out var entry))
                {
                    dictionary[key] = (entry.Value, DateTime.UtcNow);
                    return entry.Value;
                }
                return default;
            }
        }

        /// <summary>
        /// Retrieves a list of values contained within the thread-safe dictionary
        /// </summary>
        /// <returns></returns>
        public List<TValue> ToList()
        {
            ThrowIfDisposed();

            lock (padlock)
            {
                return dictionary.Values.Select(entry => entry.Value).ToList();
            }
        }

        /// <summary>
        /// Try to get the value associated with the given key, returning false if key is not found
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue? value)
        {
            ThrowIfDisposed();

            lock (padlock)
            {
                if (dictionary.TryGetValue(key, out var entry))
                {
                    dictionary[key] = (entry.Value, DateTime.UtcNow);
                    value = entry.Value;
                    return true;
                }
                value = default;
                return false;
            }
        }

        /// <summary>
        /// Remove an element by key in a thread-safe manner
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(TKey key)
        {
            ThrowIfDisposed();

            lock (padlock)
            {
                return dictionary.Remove(key);
            }
        }

        /// <summary>
        /// Clear the entire dictionary in a thread-safe manner
        /// </summary>
        public void Clear()
        {
            ThrowIfDisposed();

            lock (padlock)
            {
                dictionary.Clear();
            }
        }

        /// <summary>
        /// Check if the dictionary contains a specific key in a thread-safe manner
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
        {
            ThrowIfDisposed();

            lock (padlock)
            {
                return dictionary.ContainsKey(key);
            }
        }

        /// <summary>
        /// Dispose method for releasing resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose pattern implementation.
        /// </summary>
        void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    cts?.Cancel();

                    try
                    {
                        cleanupTask?.Wait();
                    }
                    catch (AggregateException) { }

                    lock (padlock)
                    {
                        dictionary.Clear();
                    }
                }

                disposed = true;
            }
        }

        async Task CleanupExpiredEntries()
        {
            if (expirationTime == null) return;

            while (cts != null && !cts.Token.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), cts.Token);

                ThrowIfDisposed();

                lock (padlock)
                {
                    DateTime now = DateTime.UtcNow;
                    var expiredKeys = dictionary
                        .Where(kvp => (now - kvp.Value.LastAccessed) > expirationTime)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var key in expiredKeys)
                    {
                        dictionary.Remove(key);
                    }
                }
            }
        }

        /// <summary>
        /// Finalizer to ensure resources are cleaned up if Dispose is not called
        /// </summary>
        ~CORE_TS_Dictionary()
        {
            Dispose(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(disposed, this);
    }
}