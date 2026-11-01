using System;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Builder.Interfaces;

public interface ISessionLimitsStage
{
    /// <summary>
    ///     Keeps all device sessions in memory forever (no eviction). This is the behavior of earlier versions.
    /// </summary>
    /// <returns></returns>
    IBuildingStage NoSessionLimits();

    /// <summary>
    ///     Limits the device sessions kept in memory with recommended defaults (see
    ///     <see cref="SessionLimits.CreateDefault" />): sessions idle for 30 minutes get evicted and at most 10.000 sessions
    ///     are kept. With a state machine configured, evicted sessions are restored on the next update of the device.
    /// </summary>
    /// <returns></returns>
    IBuildingStage WithDefaultSessionLimits();

    /// <summary>
    ///     Limits the device sessions kept in memory. Sessions idle for longer than
    ///     <see cref="SessionLimits.IdleTimeout" />, or exceeding <see cref="SessionLimits.MaxSessions" /> (least recently
    ///     active first), get evicted and disposed. With a state machine configured, their state is saved and restored
    ///     on the next update of the device.
    /// </summary>
    /// <param name="configure">Sets the limits.</param>
    /// <returns></returns>
    IBuildingStage WithSessionLimits(Action<SessionLimits> configure);

    /// <summary>
    ///     Limits the device sessions kept in memory. See <see cref="WithSessionLimits(Action{SessionLimits})" />.
    /// </summary>
    /// <param name="limits">The limits to use (gets copied).</param>
    /// <returns></returns>
    IBuildingStage WithSessionLimits(SessionLimits limits);
}
