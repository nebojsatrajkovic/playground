namespace Core.Shared.Services.Interfaces
{
    public interface IQueueService<T>
    {
        T Take();

        void Add(T item);

        void AddRange(List<T> items);

        int GetCount();
    }
}