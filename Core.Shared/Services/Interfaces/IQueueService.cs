
namespace Core.Shared.Services.Interfaces
{
    /// <summary>
    /// Queue service which is a wrapper for integrated C# BlockingCollection.
    /// It's intended to be used when processing data with multiple threads.
    /// By default it's thread-safe and ensures no thread will process same item again.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IQueueService<T>
    {
        /// <summary>
        /// Takes first item in the queue. If collection is empty calling thread will go to sleep.
        /// </summary>
        /// <returns></returns>
        T Take();
        /// <summary>
        /// Takes first item in the queue. If collection is empty calling thread will go to sleep.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        T Take(CancellationToken token);
        /// <summary>
        /// Tries to take item with cancellation token and default timeout
        /// </summary>
        /// <param name="token"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryTake(CancellationToken token, out T value);
        /// <summary>
        /// Takes specified number of items from collection. If there are no items in collection calling thread will go to sleep.
        /// If collection has less items than requested it will take all from the collection and return it. It will not wait until it's filled up.
        /// </summary>
        /// <param name="batchCount"></param>
        /// <returns></returns>
        List<T> TakeBatch(int batchCount);
        /// <summary>
        /// Takes specified number of items from collection. If there are no items in collection calling thread will go to sleep.
        /// If collection has less items than requested it will take all from the collection and return it. It will not wait until it's filled up.
        /// </summary>
        /// <param name="batchCount"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        List<T> TakeBatch(int batchCount, CancellationToken token);
        /// <summary>
        /// Adds item to the collection
        /// </summary>
        /// <param name="item"></param>
        void Add(T item);
        /// <summary>
        /// Adds multiple items to the collection
        /// </summary>
        /// <param name="items"></param>
        void AddRange(List<T> items);
        /// <summary>
        /// Retrieves count of items present in the collection.
        /// </summary>
        /// <returns></returns>
        int GetCount();
    }
}