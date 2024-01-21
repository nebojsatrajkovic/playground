using Core.Shared.Services.Interfaces;
using System.Collections.Concurrent;

namespace Core.Shared.Services
{
    public class QueueService<T> : IQueueService<T>
    {
        private readonly BlockingCollection<T> _Queue;

        public QueueService()
        {
            _Queue = new BlockingCollection<T>();
        }

        public T Take()
        {
            return _Queue.Take();
        }

        public void Add(T item)
        {
            _Queue.Add(item);
        }

        public void AddRange(List<T> items)
        {
            foreach (var item in items)
            {
                _Queue.Add(item);
            }
        }

        public int GetCount()
        {
            return _Queue.Count;
        }
    }
}