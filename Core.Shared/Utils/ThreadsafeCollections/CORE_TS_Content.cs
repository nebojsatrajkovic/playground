namespace Core.Shared.Utils.ThreadsafeCollections
{
    /// <summary>
    /// A thread-safe wrapper that manages concurrent updates to an encapsulated object of type <typeparamref name="T"/>. 
    /// Ensures that modifications to the object are synchronized across threads to prevent race conditions.
    /// </summary>
    /// <typeparam name="T">The type of object being wrapped, which must implement <see cref="ICloneable"/>.</typeparam>
    public class CORE_TS_Content<T> where T : ICloneable
    {
        readonly object Padlock = new();
        T content = default!;

        /// <summary>
        /// Initializes a new instance of the <see cref="CORE_TS_Content{T}"/> class 
        /// with the specified content, encapsulating the provided data.
        /// </summary>
        /// <param name="content">The content to be wrapped by this instance.</param>
        public CORE_TS_Content(T content)
        {
            if (EqualityComparer<T>.Default.Equals(content, default))
            {
                throw new ArgumentException("Content cannot be the default value for the type.", nameof(content));
            }

            this.content = content;
        }

        /// <summary>
        /// Executes the specified action while ensuring exclusive access to the content data.
        /// This method attempts to acquire a lock on the associated object and guarantees that
        /// only one thread can modify the content at a time. If the lock cannot be obtained 
        /// within the specified timeout, a <see cref="TimeoutException"/> is thrown.
        /// </summary>
        /// <param name="action">The action to be executed within the locked context.</param>
        /// /// <param name="timeoutInMilliseconds">The timeout in milliseconds to wait for acquiring the lock.</param>
        /// <exception cref="TimeoutException">Thrown if the lock cannot be acquired within the timeout period.</exception>
        public void LockingAction(Action<T> action, int timeoutInMilliseconds = 2000)
        {
            bool lockTaken = false;

            try
            {
                lockTaken = Monitor.TryEnter(Padlock, timeoutInMilliseconds);

                if (!lockTaken)
                {
                    throw new TimeoutException("Failed to acquire a lock on object on time.");
                }

                action(content);
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(Padlock);
                }
            }
        }

        /// <summary>
        /// Retrieves a cloned copy of the wrapper's content. This allows the data to be read 
        /// without modifying the original content or blocking other threads from updating the 
        /// actual value.
        /// </summary>
        /// <returns>A clone of the content, ensuring read-only access without affecting the underlying value.</returns>
        public T GetReadOnlyContent()
        {
            lock (Padlock)
            {
                return (T)content.Clone();
            }
        }
    }
}