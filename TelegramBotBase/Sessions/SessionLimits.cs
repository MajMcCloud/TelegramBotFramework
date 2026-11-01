using System;

namespace TelegramBotBase.Sessions;

/// <summary>
///     Limits for the device sessions kept in memory. Without any limit configured (the default) sessions are kept
///     forever.
/// </summary>
/// <remarks>
///     Evicted sessions are disposed silently (no <c>OnClosed</c> handler is called and no message gets sent or
///     deleted). If a state machine is configured, the state of an evicted session is saved and restored on the
///     next update of that device; otherwise the next update starts a fresh session with the start form.
///     Sessions currently processing an update, and sessions with a form marked by
///     <see cref="Attributes.KeepAlive" /> or <see cref="Form.FormBase.CanEvict" /> returning <c>false</c>, are never
///     evicted.
/// </remarks>
public sealed class SessionLimits
{
    /// <summary>
    ///     Sessions without any update for longer than this get evicted. <c>null</c> disables idle eviction.
    /// </summary>
    public TimeSpan? IdleTimeout { get; set; }

    /// <summary>
    ///     Maximum number of sessions kept in memory. When exceeded, the least recently active sessions get evicted.
    ///     <c>null</c> disables the limit.
    /// </summary>
    /// <remarks>
    ///     The limit can be exceeded temporarily, when all sessions are in use or opted out of eviction.
    /// </remarks>
    public int? MaxSessions { get; set; }

    /// <summary>
    ///     How often the background sweeper looks for sessions to evict. Defaults to one minute.
    /// </summary>
    public TimeSpan SweepInterval { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    ///     Returns if any limit is configured.
    /// </summary>
    public bool IsEnabled => IdleTimeout != null || MaxSessions != null;

    internal void Validate()
    {
        if (IdleTimeout is { } idle && idle <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(IdleTimeout), idle, "The idle timeout has to be positive.");
        }

        if (MaxSessions is { } max && max < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxSessions), max, "The maximum number of sessions has to be at least 1.");
        }

        if (SweepInterval <= TimeSpan.Zero || SweepInterval.TotalMilliseconds > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(SweepInterval), SweepInterval,
                                                  $"The sweep interval has to be positive and at most {int.MaxValue} ms.");
        }
    }

    internal SessionLimits Clone()
    {
        return new SessionLimits
        {
            IdleTimeout = IdleTimeout,
            MaxSessions = MaxSessions,
            SweepInterval = SweepInterval
        };
    }
}
