using System.Collections.Concurrent;

namespace FileWatcher
{
    /// <summary>
    /// Collects messages within a configurable time window and sends them as a single combined message.
    /// The timer starts when the first message arrives; all messages collected until the timer fires
    /// are sent together.
    /// </summary>
    public class MessageBatcher
    {
        private readonly ConcurrentQueue<string> _queue = new();
        private readonly Func<string, Task> _sendAction;
        private readonly int _intervalMs;
        private Timer? _timer;
        private readonly object _lock = new();

        public MessageBatcher(int intervalSeconds, Func<string, Task> sendAction)
        {
            _intervalMs = intervalSeconds * 1000;
            _sendAction = sendAction;
        }

        public void Enqueue(string message)
        {
            _queue.Enqueue(message);

            lock (_lock)
            {
                // Start the timer only if it's not already running.
                // Subsequent messages within the window are simply queued.
                _timer ??= new Timer(_ => Flush(), null, _intervalMs, Timeout.Infinite);
            }
        }

        private async void Flush()
        {
            lock (_lock)
            {
                _timer?.Dispose();
                _timer = null;
            }

            var messages = new List<string>();

            while (_queue.TryDequeue(out var msg))
            {
                messages.Add(msg);
            }

            if (messages.Count == 0)
                return;

            var combined = string.Join(Environment.NewLine, messages);

            try
            {
                await _sendAction(combined);
            }
            catch
            {
                // Sending failed – messages are already dequeued, avoid crash.
            }
        }
    }
}
