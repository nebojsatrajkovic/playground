namespace Core.Shared.Utils.ThreadsafeCollections
{
    public class CORE_TS_Dictionary<TKey, TValue> where TKey : notnull
    {
        Dictionary<TKey, TValue> dictionary = [];
        private readonly object padlock = new();

        /// <summary>
        /// Get the count of elements in a thread-safe manner.
        /// </summary>
        public int Count
        {
            get
            {
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
        public TValue Get(TKey key)
        {
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
            lock (padlock)
            {
                return dictionary.ContainsKey(key);
            }
        }
    }
}