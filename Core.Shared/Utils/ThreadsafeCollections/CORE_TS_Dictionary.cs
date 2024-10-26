namespace Core.Shared.Utils.ThreadsafeCollections
{
    public class CORE_TS_Dictionary<TKey, TValue> : IDisposable where TKey : notnull
    {
        Dictionary<TKey, TValue> dictionary = [];
        readonly object padlock = new();
        bool disposed = false;

        /// <summary>
        /// Get the count of elements in a thread-safe manner.
        /// </summary>
        public int Count
        {
            get
            {
                if (disposed) { return 0; }

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
            if (disposed) return;

            lock (padlock)
            {
                dictionary.Add(key, value);
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
            if (disposed) return false;

            lock (padlock)
            {
                return dictionary.TryAdd(key, value);
            }
        }

        /// <summary>
        /// Adds or updates a key-value pair in a thread-safe manner
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddOrUpdate(TKey key, TValue value)
        {
            if (disposed) return;

            lock (padlock)
            {
                dictionary[key] = value;
            }
        }

        /// <summary>
        /// Get the value associated with the given key in a thread-safe manner
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue? Get(TKey key)
        {
            if (disposed) return default;

            lock (padlock)
            {
                return dictionary[key];
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
            if (disposed)
            {
                value = default;
                return false;
            }

            lock (padlock)
            {
                return dictionary.TryGetValue(key, out value);
            }
        }

        /// <summary>
        /// Remove an element by key in a thread-safe manner
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(TKey key)
        {
            if (disposed) return false;

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
            if (disposed) return;

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
            if (disposed) return false;

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
                    lock (padlock)
                    {
                        dictionary.Clear();
                        dictionary = null!;
                    }
                }

                disposed = true;
            }
        }

        /// <summary>
        /// Finalizer to ensure resources are cleaned up if Dispose is not called
        /// </summary>
        ~CORE_TS_Dictionary()
        {
            Dispose(false);
        }
    }
}