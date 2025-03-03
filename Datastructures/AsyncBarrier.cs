namespace Beancounter.Datastructures;

public class AsyncBarrier
{
    private readonly int participantCount;
    private int currentCount;
    private TaskCompletionSource<bool> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public AsyncBarrier(int participantCount)
    {
        if (participantCount <= 0)
            throw new ArgumentException("Participant count must be greater than zero.", nameof(participantCount));
        this.participantCount = participantCount;
    }

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