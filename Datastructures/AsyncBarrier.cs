namespace Beancounter.Datastructures;

/// <summary>
/// Provides a synchronization primitive for coordinating multiple async operations.
/// Allows a specified number of participants to wait for each other before proceeding.
/// </summary>
public class AsyncBarrier
{
    private readonly int participantCount;
    private int currentCount;
    private TaskCompletionSource<bool> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Initializes a new instance of the AsyncBarrier class with the specified number of participants.
    /// </summary>
    /// <param name="participantCount">The number of participants that must signal before all can proceed.</param>
    /// <exception cref="ArgumentException">Thrown when participantCount is less than or equal to zero.</exception>
    public AsyncBarrier(int participantCount)
    {
        if (participantCount <= 0)
            throw new ArgumentException("Participant count must be greater than zero.", nameof(participantCount));
        this.participantCount = participantCount;
    }

    /// <summary>
    /// Signals that this participant has reached the barrier and waits for all other participants.
    /// When the last participant calls this method, all waiting participants are released.
    /// The barrier automatically resets for the next round.
    /// </summary>
    /// <returns>A Task that completes when all participants have signaled.</returns>
    public Task SignalAndWaitAsync()
    {
        // Capture the current TCS to await on it.
        var tcsToAwait = tcs;

        if (Interlocked.Increment(ref currentCount) == participantCount)
        {
            // When the last participant arrives, signal the barrier and prepare for the next cycle.
            tcsToAwait.TrySetResult(true);
            // Reset for the next round
            tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Interlocked.Exchange(ref currentCount, 0);
        }

        // Wait on the TCS that represents the current barrier cycle.
        return tcsToAwait.Task;
    }
}