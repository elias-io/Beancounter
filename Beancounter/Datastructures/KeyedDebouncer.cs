using System;
using System.Collections.Concurrent;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Beancounter.Datastructures;

public sealed class KeyedDebouncer : IDisposable
{
    private readonly TimeSpan _delay;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _tokens = new();

    public KeyedDebouncer(TimeSpan delay)
    {
        _delay = delay;
    }

    /// <summary>
    /// Debounce an async action identified by a key.
    /// The action runs only after no new calls for the delay period.
    /// </summary>
    public void Debounce(string key, Func<Task> action)
    {
        var newCts = new CancellationTokenSource();

        var oldCts = _tokens.AddOrUpdate(
            key,
            newCts,
            (_, existing) =>
            {
                existing.Cancel();
                existing.Dispose();
                return newCts;
            });

        // Start background task
        _ = RunDebouncedAsync(key, newCts, action);
    }

    private async Task RunDebouncedAsync(string key, CancellationTokenSource cts, Func<Task> action)
    {
        try {
            await Task.Delay(_delay, cts.Token);

            if (!cts.IsCancellationRequested) {
                await action();
            }
        }
        catch (TaskCanceledException) {
            // expected when debounced again
        }
        finally
        {
            // clean up only if this CTS is still the current one for the key
            _tokens.TryGetValue(key, out var current);
            if (current == cts)
            {
                _tokens.TryRemove(key, out _);
            }

            cts.Dispose();
        }
    }

    public void Dispose()
    {
        foreach (var kv in _tokens)
        {
            kv.Value.Cancel();
            kv.Value.Dispose();
        }

        _tokens.Clear();
    }
}