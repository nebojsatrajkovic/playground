namespace Core.Shared.Utils.Threads.Collections.ContentWrapper
{
    /// <summary>
    /// This class bears the object plus a locking object. Through this object
    /// the locking mechano of Thread Safe Collections can be more finegrained
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Core_TSC_ContentItem<T>
    {
        private object padlock = new();

        private T value { get; set; }

        public Core_TSC_ContentItem(T value)
        {
            this.value = value;
        }

        /// <summary>
        /// Used to threadsafe retrieval of the value
        /// </summary>
        /// <returns></returns>
        public T GetValue()
        {
            lock (padlock)
            {
                return value;
            }
        }

        /// <summary>
        /// Used to threadsafe retrieval of the value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(out T value)
        {
            lock (padlock)
            {
                value = this.value;
            }

            return value != null;
        }

        /// <summary>
        /// Used to set the value to a specified value
        /// </summary>
        /// <param name="newValue"></param>
        public void SetValue(T newValue)
        {
            lock (padlock)
            {
                value = newValue;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCheckAndManipulateFunction"></param>
        public T CheckAndManipulateValue(Func<T, T> pCheckAndManipulateFunction)
        {
            lock (padlock)
            {
                value = pCheckAndManipulateFunction(value);
                return value;
            }
        }
    }
}