namespace Core.Shared.Utils.ThreadsafeCollections
{
    public class CORE_TS_List<T>
    {
        readonly List<T> list = [];
        readonly object padlock = new();

        /// <summary>
        /// Number of elements within the collection.
        /// </summary>
        public int Count { get { lock (padlock) { return list.Count; } } }

        /// <summary>
        /// Adds item to the collection.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            lock (padlock)
            {
                list.Add(item);
            }
        }

        /// <summary>
        /// Adds items to the collection.
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<T> items)
        {
            lock (padlock)
            {
                foreach (var item in items)
                {
                    list.Add(item);
                }
            }
        }

        /// <summary>
        /// Gets the element at the specified index.
        /// Throws an exception if out of range.
        /// </summary>
        /// <param name="index">Index position of the element.</param>
        /// <returns>Item of the collection at the specified index.</returns>
        public T Get(int index)
        {
            lock (padlock)
            {
                return list[index];
            }
        }

        /// <summary>
        /// Removes an item from the collection at the specified index.
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            lock (padlock)
            {
                list.RemoveAt(index);
            }
        }

        /// <summary>
        /// Removes an item from the collection.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>True if an item has been removed.</returns>
        public bool Remove(T item)
        {
            lock (padlock)
            {
                return list.Remove(item);
            }
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public void Clear()
        {
            lock (padlock)
            {
                list.Clear();
            }
        }
    }
}