using System.Runtime.CompilerServices;

namespace Core.Shared.Utils.ThreadsafeCollections
{
    public class CORE_TS_SlimDictionary<TKey, TValue> : IDisposable where TKey : notnull
    {
        private volatile bool isDisposing;

        private readonly ReaderWriterLockSlim _lock;

        private Dictionary<TKey, TValue> collection;

        public CORE_TS_SlimDictionary(int initialCapacity = 0)
        {
            isDisposing = false;
            _lock = new();
            collection = initialCapacity > 0 ? new Dictionary<TKey, TValue>(initialCapacity) : [];
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
                collection[key] = value;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool TryGetValue(TKey key, out TValue? value)
        {
            ThrowIfDisposed();

            _lock.EnterReadLock();

            try
            {
                return collection.TryGetValue(key, out value);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void RemoveExpiredKeys(Func<TValue, bool> isExpired)
        {
            ThrowIfDisposed();

            var keysToRemove = new List<TKey>();

            _lock.EnterReadLock();

            try
            {
                foreach (var kvp in collection)
                {
                    if (isExpired(kvp.Value))
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
            GC.SuppressFinalize(this);

            if (isDisposing) return;
            isDisposing = true;

            _lock.EnterWriteLock();

            try
            {
                collection.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            _lock.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(isDisposing, this);
    }
}