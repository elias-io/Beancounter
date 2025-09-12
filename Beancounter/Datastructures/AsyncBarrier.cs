using System.Collections.Concurrent;

namespace Beancounter.Datastructures;
public sealed class AsyncBarrier : IAsyncDisposable, IDisposable
{
    private readonly int participants;
    private readonly int parallel;
    private readonly object @lock = new();

    private int created;
    private bool disposed;

    private sealed class Phase(int parallel)
    {
        public int Arrivals;
        public readonly TaskCompletionSource<bool> Open =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        public readonly SemaphoreSlim Slots = new(parallel, parallel);
    }

    private sealed class ParticipantState
    {
        public int PhaseIndex;
        public SemaphoreSlim? Held;               // slot held until next call
        public readonly SemaphoreSlim Gate = new(1, 1); // serialize calls per participant
    }

    private readonly Dictionary<int, Phase> phases = new();
    private readonly ConcurrentDictionary<string, ParticipantState> states =
        new(StringComparer.Ordinal);

    public AsyncBarrier(int participantCount, int parallelExecutions)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(participantCount);
        ArgumentOutOfRangeException.ThrowIfNegative(parallelExecutions);
        if (parallelExecutions == 0 || parallelExecutions > participantCount)
            parallelExecutions = participantCount;

        participants = participantCount;
        parallel = parallelExecutions;
    }

    public async Task SignalAndWaitAsync(string participantId, string step, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(participantId)) throw new ArgumentException("participantId required", nameof(participantId));
        if (string.IsNullOrWhiteSpace(step)) throw new ArgumentException("step required", nameof(step));
        ThrowIfDisposed();

        var state = states.GetOrAdd(participantId, _ =>
        {
            if (Interlocked.Increment(ref created) > participants)
            {
                Interlocked.Decrement(ref created);
                throw new InvalidOperationException("More participants than participantCount.");
            }
            return new ParticipantState();
        });

        await state.Gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            // Release previous phase slot if we were holding one.
            Interlocked.Exchange(ref state.Held, null)?.Release();

            // Get this participant's current phase.
            var phase = GetOrCreatePhase(state.PhaseIndex);

            // Arrive; last arrival opens the phase.
            Arrive(phase);

            // Wait for phase to open, then take one of the allowed parallel slots.
            await phase.Open.Task.WaitAsync(ct).ConfigureAwait(false);
            await phase.Slots.WaitAsync(ct).ConfigureAwait(false);

            // Hold slot until next call from the same participant.
            state.Held = phase.Slots;

            // Advance to the next phase for this participant.
            Interlocked.Increment(ref state.PhaseIndex);
        }
        finally
        {
            state.Gate.Release();
        }
    }

    private Phase GetOrCreatePhase(int index)
    {
        lock (@lock)
        {
            if (!phases.TryGetValue(index, out var phase))
            {
                phase = new Phase(parallel);
                phases[index] = phase;
            }
            return phase;
        }
    }

    private void Arrive(Phase phase)
    {
        bool openNow = false;
        lock (@lock)
        {
            phase.Arrivals++;
            if (phase.Arrivals == participants) openNow = true;
        }
        if (openNow) phase.Open.TrySetResult(true);
    }

    private void ThrowIfDisposed()
    {
        if (disposed) throw new ObjectDisposedException(nameof(AsyncBarrier));
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;

        // Release any held slots and dispose participant gates.
        foreach (var kv in states)
        {
            Interlocked.Exchange(ref kv.Value.Held, null)?.Release();
            kv.Value.Gate.Dispose();
        }
        states.Clear();

        // Dispose phase semaphores.
        lock (@lock)
        {
            foreach (var p in phases.Values) p.Slots.Dispose();
            phases.Clear();
        }
    }

    public ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }
}