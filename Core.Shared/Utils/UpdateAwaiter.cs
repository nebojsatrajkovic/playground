using Core.Shared.Models;

namespace Core.Shared.Utils
{
    /// <summary>
    /// Handler which awaits update to occur for any kind of operation that is affected by an external component for example.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expirationTimestampUtc">Specifies duration of the await period after which operation will timeout.</param>
    public class UpdateAwaiter<T>(DateTime expirationTimestampUtc)
    {
        /// <summary>
        /// Task that will be used to signal that the update has been received
        /// </summary>
        private readonly TaskCompletionSource<T> _tcs = new();
        /// <summary>
        /// Specifies timestamp after which the operation will time out.
        /// </summary>
        private readonly DateTime _expirationTimestampUtc = expirationTimestampUtc;

        /// <summary>
        /// Sets the update awaiting task with a result which will automatically mark it as completed.
        /// </summary>
        /// <param name="update">External update payload.</param>
        public void OnUpdateReceived(T update)
        {
            if (!_tcs.Task.IsCompleted)
            {
                _tcs.SetResult(update);
            }
        }

        /// <summary>
        /// Method that awaits either the update arrival or timeout.
        /// </summary>
        /// <returns>Returns an update that has been received.</returns>
        public async Task<ResultOf<T>> AwaitUpdate_Async()
        {
            try
            {
                // task which will complete after specified timeout
                var timeoutTask = Task.Delay(_expirationTimestampUtc - DateTime.UtcNow);

                // wait for either a timeout task completion or answer received task completion
                var completedTask = await Task.WhenAny(_tcs.Task, timeoutTask);

                // if first completed task is answer received task
                if (completedTask == _tcs.Task)
                {
                    // take the notification from the task result
                    var update = _tcs.Task.Result;

                    return new ResultOf<T>(update);
                }
                else
                {
                    // RPC invocation has timed out
                    return new ResultOf<T>(CORE_OperationStatus.TIMED_OUT);
                }
            }
            catch (Exception ex)
            {
                return new ResultOf<T>(ex);
            }
        }
    }
}