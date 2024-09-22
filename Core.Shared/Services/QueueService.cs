using Core.Shared.Services.Interfaces;
using System.Collections.Concurrent;

namespace Core.Shared.Services
{
    /// <summary>
    /// Queue service which is a wrapper for integrated C# BlockingCollection.
    /// It's intended to be used when processing data with multiple threads.
    /// By default it's thread-safe and ensures no thread will process same item again.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueueService<T> : IQueueService<T>
    {
        /// <summary>
        /// C# blocking collection
        /// </summary>
        readonly BlockingCollection<T> _Queue;

        public QueueService()
        {
            _Queue = [];
        }

        /// <inheritdoc/>
        public T Take()
        {
            return _Queue.Take();
        }

        /// <inheritdoc/>
        public T Take(CancellationToken token)
        {
            return _Queue.Take(token);
        }

        /// <inheritdoc/>
        public bool TryTake(CancellationToken token, out T? value)
        {
            return _Queue.TryTake(out value, 10 * 1000, token);
        }

        /// <inheritdoc/>
        public List<T> TakeBatch(int batchCount)
        {
            return TakeBatch_Internal(batchCount);
        }

        /// <inheritdoc/>
        public List<T> TakeBatch(int batchCount, CancellationToken token)
        {
            return TakeBatch_Internal(batchCount, token);
        }

        /// <inheritdoc/>
        public void Add(T item)
        {
            _Queue.Add(item);
        }

        /// <inheritdoc/>
        public void AddRange(List<T> items)
        {
            lock (_Queue)
            {
                foreach (var item in items)
                {
                    _Queue.Add(item);
                }
            }
        }

        /// <inheritdoc/>
        public int GetCount()
        {
            return _Queue.Count;
        }

        /// <summary>
        /// Private method that returns requested number of items present in collection. Or all of those present if requested number is larger than collection count.
        /// </summary>
        /// <param name="batchCount"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        List<T> TakeBatch_Internal(int batchCount, CancellationToken? token = null)
        {
            if (token?.IsCancellationRequested == true) { return []; }

            var first = token != null && token.HasValue ? _Queue.Take(token.Value) : _Queue.Take();

            var batch = new List<T> { first };

            for (int i = 0; i < batchCount - 1; i++)
            {
                if (token?.IsCancellationRequested == true) { return []; }

                if (_Queue.TryTake(out var item))
                {
                    batch.Add(item);
                }
                else
                {
                    // Queue is empty, break the loop
                    break;
                }
            }
            return batch;
        }
    }
}