using TelegramBotBase.Base;

namespace TelegramBotBase.Interfaces;

/// <summary>
///     A state machine which can save, load and remove the state of a single device.
/// </summary>
/// <remarks>
///     When session limits are configured (see <see cref="Sessions.SessionLimits" />), the framework uses these methods
///     instead of the bulk ones: states are not loaded at startup but on the first update of each device, evicted
///     sessions are saved one by one, and saving all sessions only updates the states of the active sessions instead
///     of replacing all stored states.
///     Without session limits, the bulk methods of <see cref="IStateMachine" /> are used as before.
/// </remarks>
public interface IDeviceStateMachine : IStateMachine
{
    /// <summary>
    ///     Saves (inserts or replaces) the state of one device.
    /// </summary>
    /// <param name="state"></param>
    void SaveFormState(StateEntry state);

    /// <summary>
    ///     Loads the state of one device.
    /// </summary>
    /// <param name="deviceId"></param>
    /// <returns>The state, or <c>null</c> if there is none.</returns>
    StateEntry LoadFormState(long deviceId);

    /// <summary>
    ///     Removes the state of one device. Does nothing if there is none.
    /// </summary>
    /// <param name="deviceId"></param>
    void RemoveFormState(long deviceId);
}
