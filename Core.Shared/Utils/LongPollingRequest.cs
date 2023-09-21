namespace Core.Shared.Utils
{
    public class LongPollingRequest
    {
        readonly int _timeout;

        static readonly List<LongPollingRequest> _requests = new();
        readonly TaskCompletionSource<bool> _taskCompletionSource = new();

        string _Channel { get; set; }
        string _Key { get; set; }
        LongPollingService_Response? _Data { get; set; }

        public LongPollingRequest(string channel, string key, int timeout = 60000)
        {
            _timeout = timeout;
            _Channel = channel.ToLower();
            _Key = key.ToLower();

            lock (_requests)
            {
                _requests.Add(this);
            }
        }

        public static void Update(string channel, string key, LongPollingService_Response? data = null)
        {
            lock (_requests)
            {
                var requests = _requests.Where(x => x._Channel.ToLower().Equals(channel.ToLower()) && x._Key.ToLower().Equals(key.ToLower())).ToList();

                foreach (var request in requests)
                {
                    request._Data = data ?? new LongPollingService_Response { IsUpdateAvailable = true };
                    request._taskCompletionSource.SetResult(true);
                }
            }
        }


        public async Task<LongPollingService_Response> WaitAsync()
        {
            await Task.WhenAny(_taskCompletionSource.Task, Task.Delay(_timeout + 5000)); // 5 seconds are added to compensate eventual FE timeout handling

            lock (_requests)
            {
                _requests.Remove(this);
            }

            return _Data ?? new LongPollingService_Response { IsTimeoutReached = true };
        }
    }

    public class LongPollingService_Response
    {
        public bool IsUpdateAvailable { get; set; }
        public bool IsTimeoutReached { get; set; }
    }
}