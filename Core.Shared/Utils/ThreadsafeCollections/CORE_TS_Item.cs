namespace Core.Shared.Utils.ThreadsafeCollections
{
    public class CORE_TS_Item<T>
    {
        readonly object padlock = new();
        T? value;

        public CORE_TS_Item(T value)
        {
            this.value = value;
        }

        /// <summary>
        /// Gets or sets the value in a thread-safe manner.
        /// </summary>
        public T? Value
        {
            get
            {
                lock (padlock)
                {
                    return value;
                }
            }
            set
            {
                lock (padlock)
                {
                    this.value = value;
                }
            }
        }
    }
}