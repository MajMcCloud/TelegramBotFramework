using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.RequestDispatchers;

/// <summary>
/// Ordered wait list for dispatch slots. Waiting requests are granted slots by priority, then by arrival order.
/// Requests blocked only by their own chat limit are skipped so they never stall other chats, and long-waiting
/// requests are gradually promoted so low priorities cannot starve
/// </summary>
internal sealed class PrioritySlotQueue
{
    /// <summary>Identity of one logical dispatch; kept across retries so a retried request does not lose its place</summary>
    internal sealed class Ticket
    {
        public RequestPriority Priority { get; }
        public long Sequence { get; }
        public long EnqueuedAt { get; }

        public Ticket(RequestPriority priority, long sequence, long enqueuedAt)
        {
            Priority = priority;
            Sequence = sequence;
            EnqueuedAt = enqueuedAt;
        }
    }

    private sealed class Pending
    {
        public Ticket Ticket;
        public IDeviceSession Session;
        public long DeviceId;
        public int Level;
        public TaskCompletionSource<bool> Completion;
        public LinkedListNode<Pending> Node;
        public CancellationTokenRegistration Registration;
        public bool Done;
    }

    private static readonly int LevelCount = Enum.GetValues(typeof(RequestPriority)).Length;

    private readonly object _lock = new();
    private readonly LinkedList<Pending>[] _lists;
    private readonly Func<IDeviceSession, SlotAttempt> _tryOccupy;
    private readonly long _agingTicks;
    private long _sequence;
    private int _count;

    /// <param name="tryOccupy">Non-blocking slot acquisition, invoked while the queue lock is held</param>
    /// <param name="agingInterval">Wait time after which a request is promoted by one level; zero or less disables aging</param>
    public PrioritySlotQueue(Func<IDeviceSession, SlotAttempt> tryOccupy, TimeSpan agingInterval)
    {
        _tryOccupy = tryOccupy;
        _agingTicks = agingInterval > TimeSpan.Zero ? (long)(agingInterval.TotalSeconds * Stopwatch.Frequency) : 0;
        _lists = new LinkedList<Pending>[LevelCount];
        for (var i = 0; i < LevelCount; i++)
        {
            _lists[i] = new LinkedList<Pending>();
        }
    }

    public Ticket CreateTicket(RequestPriority priority)
    {
        return new Ticket(priority, Interlocked.Increment(ref _sequence), Stopwatch.GetTimestamp());
    }

    /// <summary>Completes once a slot has been taken for <paramref name="ds"/>; the caller then owns that slot</summary>
    public Task WaitForSlot(IDeviceSession ds, Ticket ticket, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            return Task.FromCanceled(ct);
        }

        var pending = new Pending
        {
            Ticket = ticket,
            Session = ds,
            DeviceId = ds.DeviceId,
            Level = Math.Max(0, Math.Min(LevelCount - 1, (int)ticket.Priority)),
            Completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously)
        };

        lock (_lock)
        {
            Insert(pending);
        }

        if (ct.CanBeCanceled)
        {
            pending.Registration = ct.Register(() => Cancel(pending, ct));
            if (pending.Completion.Task.IsCompleted)
            {
                pending.Registration.Dispose();
            }
        }

        Drain();
        return pending.Completion.Task;
    }

    /// <summary>Hands out as many slots as currently possible, best waiting request first</summary>
    public void Drain()
    {
        List<Pending> granted = null;

        lock (_lock)
        {
            if (_count == 0)
            {
                return;
            }

            var now = Stopwatch.GetTimestamp();
            var cursors = new LinkedListNode<Pending>[LevelCount];
            for (var i = 0; i < LevelCount; i++)
            {
                cursors[i] = _lists[i].First;
            }

            HashSet<long> blockedSessions = null;

            while (true)
            {
                var best = -1;
                var bestLevel = int.MaxValue;
                var bestSequence = long.MaxValue;

                for (var i = 0; i < LevelCount; i++)
                {
                    var node = cursors[i];
                    if (node == null)
                    {
                        continue;
                    }

                    var level = EffectiveLevel(node.Value, now);
                    var sequence = node.Value.Ticket.Sequence;
                    if (level < bestLevel || (level == bestLevel && sequence < bestSequence))
                    {
                        best = i;
                        bestLevel = level;
                        bestSequence = sequence;
                    }
                }

                if (best < 0)
                {
                    break;
                }

                var current = cursors[best];
                cursors[best] = current.Next;
                var pending = current.Value;

                if (blockedSessions != null && blockedSessions.Contains(pending.DeviceId))
                {
                    continue;
                }

                var attempt = _tryOccupy(pending.Session);
                if (attempt == SlotAttempt.BlockedGlobally)
                {
                    break;
                }

                if (attempt == SlotAttempt.BlockedForSession)
                {
                    blockedSessions ??= new HashSet<long>();
                    blockedSessions.Add(pending.DeviceId);
                    continue;
                }

                _lists[pending.Level].Remove(current);
                pending.Node = null;
                pending.Done = true;
                _count--;
                (granted ??= new List<Pending>()).Add(pending);
            }
        }

        if (granted == null)
        {
            return;
        }

        foreach (var pending in granted)
        {
            pending.Registration.Dispose();
            pending.Completion.TrySetResult(true);
        }
    }

    private void Insert(Pending pending)
    {
        var list = _lists[pending.Level];

        // New tickets carry the highest sequence so this is a tail append; retries walk back to their old position
        var after = list.Last;
        while (after != null && after.Value.Ticket.Sequence > pending.Ticket.Sequence)
        {
            after = after.Previous;
        }

        pending.Node = after == null
            ? list.AddFirst(pending)
            : list.AddAfter(after, pending);
        _count++;
    }

    private void Cancel(Pending pending, CancellationToken ct)
    {
        lock (_lock)
        {
            if (pending.Done)
            {
                return;
            }

            _lists[pending.Level].Remove(pending.Node);
            pending.Node = null;
            pending.Done = true;
            _count--;
        }

        pending.Completion.TrySetCanceled(ct);
    }

    // Promotes long-waiting requests, but never above UI (only Critical requests start there) so aging
    // cannot make bulk traffic outrank critical work
    private int EffectiveLevel(Pending pending, long now)
    {
        if (_agingTicks <= 0)
        {
            return pending.Level;
        }

        var steps = (now - pending.Ticket.EnqueuedAt) / _agingTicks;
        var floor = Math.Min(pending.Level, (int)RequestPriority.UI);
        return (int)Math.Max(floor, pending.Level - steps);
    }
}
