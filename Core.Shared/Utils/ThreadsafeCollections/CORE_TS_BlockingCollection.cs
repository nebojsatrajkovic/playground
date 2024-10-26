namespace Core.Shared.Utils.ThreadsafeCollections
{
    /// <summary>
    /// Blocking collection that ensures thread-safe queue operations.
    /// In the background it's using semaphore so there are scenarios to aware of.
    /// Example: Add multiple items using AddAndSignalOnce method and have only one thread that invokes Take() which will retrieve a single item.
    /// In that case other items will be stuck in the collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CORE_TS_BlockingCollection<T>
    {
        object padlock;
        Queue<T> queue;
        SemaphoreSlim semaphore;

        public CORE_TS_BlockingCollection()
        {
            padlock = new object();
            queue = new Queue<T>();
            semaphore = new SemaphoreSlim(0);
        }

        /// <summary>
        /// Adds an item to the collection.
        /// After items has been added it will execute semaphore release that will enable random pending thread to continue executing.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            lock (padlock)
            {
                queue.Enqueue(item);
            }

            semaphore.Release();
        }

        /// <summary>
        /// Adds multiple items to the collection, releasing the semaphore after each item to signal multiple waiting threads.
        /// </summary>
        /// <param name="items">The list of items to add to the collection.</param>
        public void AddRange(List<T> items)
        {
            lock (padlock)
            {
                foreach (var item in items)
                {
                    queue.Enqueue(item);
                }
            }

            semaphore.Release(items.Count);
        }

        /// <summary>
        /// Takes an item from the collection. If there are no items present it will put calling thread to sleep.
        /// If cancellation token is cancelled it will return default value of the collection type.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<T?> TakeAsync(CancellationToken token = default)
        {
            try
            {
                await semaphore.WaitAsync(token);

                lock (padlock)
                {
                    if (queue.Count > 0)
                    {
                        return queue.Dequeue();
                    }

                    throw new InvalidOperationException("The collection is empty."); // should not happen
                }
            }
            catch (InvalidOperationException) // collection is closed or empty
            {
                return default;
            }
            catch (OperationCanceledException) // cancellation token has been cancelled
            {
                return default;
            }
            catch (Exception) // general exception
            {
                throw;
            }
        }
    }
}