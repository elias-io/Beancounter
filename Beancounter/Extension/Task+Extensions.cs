using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Beancounter.Extension;

/// <summary>
/// Extensions for working with <see cref="Task"/> collections.
/// </summary>
public static class Task_Extensions
{
    /// <summary>
    /// Await all tasks if they all succeed; otherwise propagate the first observed exception immediately.
    /// Unlike <see cref="Task.WhenAll(System.Collections.Generic.IEnumerable{Task})"/>,
    /// this fails fast on the first faulted task. Other task exceptions are observed to prevent
    /// <see cref="System.Threading.Tasks.UnobservedTaskException"/>.
    /// </summary>
    /// <param name="tasks">The tasks to await.</param>
    /// <param name="cancellationToken">Optional token to cancel the awaiting operation.</param>
    public static async Task WhenAllOrAnyThrows(this IEnumerable<Task> tasks, CancellationToken cancellationToken = default)
    {
        var taskList = tasks as IList<Task> ?? tasks.ToList();
        if (taskList.Count == 0) return;

        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

        foreach (var task in taskList)
        {
            // If a task faults, surface that exception right away and also observe it to avoid UnobservedTaskException.
            task.ContinueWith(t =>
            {
                _ = t.Exception; // observe
                if (t.Exception != null)
                {
                    tcs.TrySetException(t.Exception.InnerExceptions);
                }
            }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

            // If any task is canceled, consider the combined operation canceled.
            task.ContinueWith(_ =>
            {
                tcs.TrySetCanceled();
            }, TaskContinuationOptions.OnlyOnCanceled | TaskContinuationOptions.ExecuteSynchronously);
        }

        if (cancellationToken.CanBeCanceled)
        {
            cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
        }

        var allTask = Task.WhenAll(taskList);
        var completed = await Task.WhenAny(allTask, tcs.Task).ConfigureAwait(false);

        if (completed == allTask)
        {
            // All tasks completed successfully.
            await allTask.ConfigureAwait(false);
            return;
        }

        // Propagate the first observed error or cancellation immediately.
        await tcs.Task.ConfigureAwait(false);
    }

    /// <summary>
    /// Generic variant that returns results if all succeed; otherwise fails fast on first faulted task.
    /// </summary>
    /// <param name="tasks">The tasks to await.</param>
    /// <param name="cancellationToken">Optional token to cancel the awaiting operation.</param>
    /// <typeparam name="T">The task result type.</typeparam>
    /// <returns>Array of results in the same order as the input tasks, if all succeed.</returns>
    public static async Task<T[]> WhenAllOrAnyThrows<T>(this IEnumerable<Task<T>> tasks, CancellationToken cancellationToken = default)
    {
        var taskList = tasks as IList<Task<T>> ?? tasks.ToList();
        if (taskList.Count == 0) return Array.Empty<T>();

        // Reuse non-generic logic for fail-fast behavior while observing other exceptions.
        await WhenAllOrAnyThrows(taskList.Select(t => (Task)t), cancellationToken).ConfigureAwait(false);
        return await Task.WhenAll(taskList).ConfigureAwait(false);
    }
}



