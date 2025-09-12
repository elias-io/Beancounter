using System.Collections.Concurrent;
using System.Diagnostics;
using Beancounter.Datastructures;

namespace Beancounter.Test;

public class AsyncBarrierTests
{
    [Test]
    public void Ctor_Throws_On_Invalid_Arguments()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AsyncBarrier(0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AsyncBarrier(-1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new AsyncBarrier(2, -1));
    }

    [Test]
    public async Task SignalAndWaitAsync_Throws_On_Invalid_Params()
    {
        await using var barrier = new AsyncBarrier(1, 1);
        Assert.ThrowsAsync<ArgumentException>(async () => await barrier.SignalAndWaitAsync("", "step"));
        Assert.ThrowsAsync<ArgumentException>(async () => await barrier.SignalAndWaitAsync("id", ""));
        Assert.ThrowsAsync<ArgumentException>(async () => await barrier.SignalAndWaitAsync(" ", "step"));
        Assert.ThrowsAsync<ArgumentException>(async () => await barrier.SignalAndWaitAsync("id", " "));
    }

    [Test]
    public async Task More_Than_Configured_Participants_Throws()
    {
        await using var barrier = new AsyncBarrier(2, 2);

        var t1 = barrier.SignalAndWaitAsync("p1", "s0");
        var t2 = barrier.SignalAndWaitAsync("p2", "s0");

        await Task.WhenAll(t1, t2);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await barrier.SignalAndWaitAsync("p3", "s0"));
        Assert.That(ex!.Message, Does.Contain("More participants"));
    }

    [Test]
    public async Task All_Participants_Block_Until_Party_Count_Reached()
    {
        await using var barrier = new AsyncBarrier(3, 3);
        var started = new CountdownEvent(3);
        var passed = new ConcurrentBag<int>();

        async Task Run(string id, int idx)
        {
            started.Signal();
            await barrier.SignalAndWaitAsync(id, "phase0");
            passed.Add(idx);
        }

        var tasks = new[]
        {
            Task.Run(() => Run("A", 1)),
            Task.Run(() => Run("B", 2)),
            Task.Run(() => Run("C", 3))
        };

        // Ensure all three started and are likely waiting
        Assert.That(started.Wait(TimeSpan.FromSeconds(3)), Is.True);

        await Task.WhenAll(tasks);
        Assert.That(passed.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task Parallel_Slots_Limit_Throughput_Per_Phase()
    {
        var participants = 5;
        var parallel = 2;
        await using var barrier = new AsyncBarrier(participants, parallel);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var inPhase = 0;
        var maxConcurrent = 0;

        async Task Run(string id)
        {
            try
            {
                // Single phase to measure concurrency; we will cancel remaining waiters afterward to avoid hangs
                await barrier.SignalAndWaitAsync(id, "p0", cts.Token);
                var now = Interlocked.Increment(ref inPhase);
                InterlockedExtensions.Max(ref maxConcurrent, now);
                await Task.Delay(75, cts.Token);
                Interlocked.Decrement(ref inPhase);
            }
            catch (OperationCanceledException)
            {
                // Expected if still waiting due to limited parallelism when we cancel the test
            }
        }

        var tasks = Enumerable.Range(1, participants)
            .Select(i => Task.Run(() => Run($"p{i}")))
            .ToArray();

        var all = Task.WhenAll(tasks);
        var completed = await Task.WhenAny(all, Task.Delay(TimeSpan.FromSeconds(10))) == all;

        // If not all completed (expected due to held slots), cancel to unblock and wait for graceful end
        if (!completed)
        {
            cts.Cancel();
            try { await all; } catch { /* ignore cancellations */ }
        }

        Assert.That(maxConcurrent, Is.LessThanOrEqualTo(parallel));
    }

    [Test]
    public async Task Slot_Is_Held_Across_Phases_By_Same_Participant()
    {
        var participants = 2;
        var parallel = 1; // only one slot should run at a time, and it should be reused by same participant across phases
        await using var barrier = new AsyncBarrier(participants, parallel);

        var order = new List<string>();
        var gate = new SemaphoreSlim(1, 1);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        async Task Run(string id)
        {
            try
            {
                // phase 0
                await barrier.SignalAndWaitAsync(id, "p0", cts.Token);
                await gate.WaitAsync();
                order.Add($"{id}-0");
                gate.Release();

                // phase 1
                await barrier.SignalAndWaitAsync(id, "p1", cts.Token);
                await gate.WaitAsync();
                order.Add($"{id}-1");
                gate.Release();

                // Start a third call that cancels shortly after entry to release the held slot from phase 1
                using var releaseCts = new CancellationTokenSource();
                var cleanup = Task.Run(async () =>
                {
                    // Ensure we get past Gate and slot release before canceling
                    releaseCts.CancelAfter(10);
                    await barrier.SignalAndWaitAsync(id, "cleanup", releaseCts.Token);
                });
                try { await cleanup; } catch (OperationCanceledException) { /* expected */ }
            }
            catch (OperationCanceledException)
            {
                // ignore for test cleanup
            }
        }

        var tasks = new[]
        {
            Task.Run(() => Run("A")),
            Task.Run(() => Run("B"))
        };

        // Wait for completion or timeout, then assert per-participant order
        var all = Task.WhenAll(tasks);
        var completed = await Task.WhenAny(all, Task.Delay(TimeSpan.FromSeconds(10))) == all;
        Assert.That(completed, Is.True, $"Test timed out. Order so far: {string.Join(",", order)}");

        Assert.That(order.Count, Is.EqualTo(4), $"Unexpected order length: {string.Join(",", order)}");
        var a0 = order.IndexOf("A-0");
        var a1 = order.IndexOf("A-1");
        var b0 = order.IndexOf("B-0");
        var b1 = order.IndexOf("B-1");
        Assert.That(a0, Is.GreaterThanOrEqualTo(0));
        Assert.That(a1, Is.GreaterThan(a0));
        Assert.That(b0, Is.GreaterThanOrEqualTo(0));
        Assert.That(b1, Is.GreaterThan(b0));
    }

    [Test]
    public async Task Cancellation_Before_Open_Cancels_Wait()
    {
        await using var barrier = new AsyncBarrier(2, 2);
        using var cts = new CancellationTokenSource(50);

        var cancelled = Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await barrier.SignalAndWaitAsync("A", "p0", cts.Token);
        });

        Assert.That(cancelled, Is.Not.Null);
    }

    [Test]
    public async Task Dispose_Releases_Held_Slots_And_Prevents_Further_Use()
    {
        var barrier = new AsyncBarrier(2, 1);

        var t1 = Task.Run(async () =>
        {
            await barrier.SignalAndWaitAsync("A", "p0");
            await Task.Delay(100);
            await barrier.SignalAndWaitAsync("A", "p1");
        });

        var t2 = Task.Run(async () =>
        {
            await barrier.SignalAndWaitAsync("B", "p0");
            await Task.Delay(100);
            await barrier.SignalAndWaitAsync("B", "p1");
        });

        await Task.Delay(20);
        barrier.Dispose();

        await Task.WhenAll(Task.WhenAny(Task.WhenAll(t1, t2), Task.Delay(1000)));

        Assert.DoesNotThrow(() => barrier.Dispose()); // multiple dispose is safe
        Assert.ThrowsAsync<ObjectDisposedException>(async () => await barrier.SignalAndWaitAsync("C", "p0"));
    }

    [Test]
    public async Task Phase_Open_Unblocks_All_Arrivals()
    {
        await using var barrier = new AsyncBarrier(4, 0); // 0 => parallel == participants
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var reached = 0;
        async Task Run(string id)
        {
            await barrier.SignalAndWaitAsync(id, "p0", cts.Token);
            Interlocked.Increment(ref reached);
        }

        var tasks = new[]
        {
            Task.Run(() => Run("1")),
            Task.Run(() => Run("2")),
            Task.Run(() => Run("3")),
            Task.Run(() => Run("4"))
        };

        var all = Task.WhenAll(tasks);
        var completed = await Task.WhenAny(all, Task.Delay(TimeSpan.FromSeconds(10))) == all;
        Assert.That(completed, Is.True, "Phase did not complete in time");
        Assert.That(reached, Is.EqualTo(4));
    }

    [Test]
    public async Task Parallel_Zero_Normalizes_To_Participant_Count()
    {
        var participants = 3;
        await using var barrier = new AsyncBarrier(participants, 0);

        var inPhase = 0;
        var maxConcurrent = 0;

        async Task Run(string id)
        {
            await barrier.SignalAndWaitAsync(id, "p0");
            var now = Interlocked.Increment(ref inPhase);
            InterlockedExtensions.Max(ref maxConcurrent, now);
            await Task.Delay(20);
            Interlocked.Decrement(ref inPhase);
        }

        var tasks = Enumerable.Range(1, participants)
            .Select(i => Task.Run(() => Run($"p{i}")))
            .ToArray();

        await Task.WhenAll(tasks);

        Assert.That(maxConcurrent, Is.EqualTo(participants));
    }

    
}

internal static class InterlockedExtensions
{
    public static void Max(ref int location, int value)
    {
        int initial, computed;
        do
        {
            initial = Volatile.Read(ref location);
            computed = Math.Max(initial, value);
            if (computed == initial) return;
        } while (Interlocked.CompareExchange(ref location, computed, initial) != initial);
    }
}


