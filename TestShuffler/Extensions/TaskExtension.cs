using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestShuffler
{
    public static class TaskExtension
    {
        public static void Await(this Task task)
        {
            Ensure.NotNull(nameof(task), task);

            task.GetAwaiter().GetResult();
        }

        public static TValue Await<TValue>(this Task<TValue> task)
        {
            Ensure.NotNull(nameof(task), task);

            return task.GetAwaiter().GetResult();
        }

        public static async Task ExecutePeriodic(Func<Task> actionAsync, TimeSpan interval, CancellationToken cancellationToken)
        {
            Ensure.NotNull(nameof(actionAsync), actionAsync);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await actionAsync();
                }
                catch (Exception exception)
                {
                    ExceptionHandler.Handle(exception);
                }

                await Task.Delay(interval, cancellationToken);
            }
        }

        public static async Task<IReadOnlyCollection<TResult>> WhenAllAsync<TResult>(this IEnumerable<Task<TResult>> tasks)
        {
            Ensure.NotNull(nameof(tasks), tasks);

            if (!tasks.Any())
            {
                return Array.Empty<TResult>();
            }

            return await Task.WhenAll(tasks);
        }

        public static async Task WhenAllAsync(this IEnumerable<Task> tasks)
        {
            Ensure.NotNull(nameof(tasks), tasks);

            if (!tasks.Any())
            {
                return;
            }

            await Task.WhenAll(tasks);
        }
    }
}