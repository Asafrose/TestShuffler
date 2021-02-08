using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TestShuffler
{
    public abstract class Module : IAsyncDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly List<Task> _tasks;

        protected CancellationToken CancellationToken => _cancellationTokenSource.Token;

        protected Module()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _tasks = new List<Task>();
        }

        public virtual async ValueTask DisposeAsync()
        {
            _cancellationTokenSource.Cancel();

            await _tasks.WhenAllAsync();

            _cancellationTokenSource?.Dispose();
        }

        protected void RegisterPeriodicTask(Func<Task> actionAsync, TimeSpan interval)
        {
            Ensure.NotNull(nameof(actionAsync), actionAsync);

            _tasks.Add(TaskExtension.ExecutePeriodic(actionAsync, interval, CancellationToken));
        }
    }
}