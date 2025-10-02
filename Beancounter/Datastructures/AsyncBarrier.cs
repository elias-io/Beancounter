using System.Collections.Concurrent;

namespace Beancounter.Datastructures;
/// <summary>
/// A synchronization primitive for coordinating multiple asynchronous participants across phases.
/// Each participant calls <see cref="SignalAndWaitAsync"/> with a stable <c>participantId</c> and a <c>step</c>
/// identifier to arrive at the current phase. When the configured number of participants have arrived,
/// the phase opens and up to <c>parallelExecutions</c> participants are allowed to proceed concurrently.
/// A participant holds its acquired execution slot until its next call, ensuring fairness across phases.
/// </summary>
/// <remarks>
/// - The barrier advances in discrete phases. Each participant advances one phase per successful call.
/// - The <c>step</c> parameter is intended for diagnostics/traceability and does not affect coordination.
/// - If <paramref name="parallelExecutions"/> is 0 or greater than <paramref name="participantCount"/>,
///   it defaults to <paramref name="participantCount"/> (i.e., no additional throttling).
/// - Instances are thread-safe and support concurrent callers.
/// - Implements <see cref="IDisposable"/> and <see cref="IAsyncDisposable"/> to release internal semaphores.
/// </remarks>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncBarrier"/> class.
    /// </summary>
    /// <param name="participantCount">The number of participants that will coordinate at the barrier.</param>
    /// <param name="parallelExecutions">The maximum number of participants allowed to proceed concurrently after a phase opens.
    /// Use 0 to allow up to <paramref name="participantCount"/> participants (no throttling).</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="participantCount"/> is less than or equal to 0, or when <paramref name="parallelExecutions"/> is negative.
    /// </exception>
    public AsyncBarrier(int participantCount, int parallelExecutions)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(participantCount);
        ArgumentOutOfRangeException.ThrowIfNegative(parallelExecutions);
        if (parallelExecutions == 0 || parallelExecutions > participantCount)
            parallelExecutions = participantCount;

        participants = participantCount;
        parallel = parallelExecutions;
    }

    /// <summary>
    /// Signals arrival of a participant to the current phase and asynchronously waits until the phase opens.
    /// Once open, acquires one of the available parallel execution slots and returns.
    /// The acquired slot is held by this participant until its next invocation, ensuring stable throughput across phases.
    /// </summary>
    /// <param name="participantId">A stable identifier for the participant across phases. Required and must be non-empty.</param>
    /// <param name="step">A descriptive label for the logical step/phase (for diagnostics). Required and must be non-empty.</param>
    /// <param name="ct">An optional cancellation token to cancel the wait.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="participantId"/> or <paramref name="step"/> is null/whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown if more distinct participants than <c>participantCount</c> attempt to join.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the barrier has been disposed.</exception>
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

    /// <summary>
    /// Signals arrival and waits for the phase to open, then executes the provided <paramref name="work"/>
    /// delegate while holding a parallel execution slot. The slot is released when the delegate completes
    /// (successfully or with an error), avoiding last-phase stalls while still enforcing <c>parallelExecutions</c>
    /// during the actual work.
    /// </summary>
    /// <param name="participantId">Stable identifier for the participant.</param>
    /// <param name="step">Diagnostic step/phase name.</param>
    /// <param name="work">Callback executed under the acquired slot.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="ArgumentException">Thrown when parameters are invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown if more distinct participants than configured attempt to join.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the barrier has been disposed.</exception>
    public async Task SignalWaitAndRunAsync(
        string participantId,
        string step,
        Func<CancellationToken, Task> work,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(participantId)) throw new ArgumentException("participantId required", nameof(participantId));
        if (string.IsNullOrWhiteSpace(step)) throw new ArgumentException("step required", nameof(step));
        if (work is null) throw new ArgumentNullException(nameof(work));
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
            // Release any previously held slot (from SignalAndWaitAsync usage)
            Interlocked.Exchange(ref state.Held, null)?.Release();

            var phase = GetOrCreatePhase(state.PhaseIndex);

            // Arrive; last arrival opens the phase.
            Arrive(phase);

            // Wait for phase to open, then take one of the allowed parallel slots.
            await phase.Open.Task.WaitAsync(ct).ConfigureAwait(false);
            await phase.Slots.WaitAsync(ct).ConfigureAwait(false);

            try
            {
                // Advance to the next phase before executing work.
                Interlocked.Increment(ref state.PhaseIndex);

                // Execute user work while holding the slot; release when done.
                await work(ct).ConfigureAwait(false);
            }
            finally
            {
                phase.Slots.Release();
            }
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

    /// <summary>
    /// Releases resources used by the barrier. Any held execution slots are released and internal semaphores disposed.
    /// Safe to call multiple times.
    /// </summary>
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

    /// <summary>
    /// Asynchronously releases resources used by the barrier.
    /// Equivalent to calling <see cref="Dispose"/>.
    /// </summary>
    public ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }
}