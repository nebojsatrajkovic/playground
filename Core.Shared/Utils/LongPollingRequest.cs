namespace Core.Shared.Utils
{
    public class LongPollingRequest
    {
        readonly int _timeout;

        static readonly List<LongPollingRequest> _requests = [];
        readonly TaskCompletionSource<bool> _taskCompletionSource = new();

        string Channel { get; set; }
        string Key { get; set; }
        LongPollingService_Response? Data { get; set; }

        public LongPollingRequest(string channel, string key, int timeout = 60000)
        {
            _timeout = timeout;
            Channel = channel.ToLower();
            Key = key.ToLower();

            lock (_requests)
            {
                _requests.Add(this);
            }
        }

        public static void Update(string channel, string key, LongPollingService_Response? data = null)
        {
            lock (_requests)
            {
                var requests = _requests.Where(x => x.Channel.ToLower().Equals(channel.ToLower()) && x.Key.ToLower().Equals(key.ToLower())).ToList();

                foreach (var request in requests)
                {
                    request.Data = data ?? new LongPollingService_Response { IsUpdateAvailable = true };
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

            return Data ?? new LongPollingService_Response { IsTimeoutReached = true };
        }
    }

    public class LongPollingService_Response
    {
        public bool IsUpdateAvailable { get; set; }
        public bool IsTimeoutReached { get; set; }
    }
}