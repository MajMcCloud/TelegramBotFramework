using System;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Builder.Interfaces;

public interface IBuildingStage
{
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

    /// <summary>
    /// Creates a full BotBase instance with all parameters previously set.
    /// </summary>
    /// <returns></returns>
    BotBase Build();
}