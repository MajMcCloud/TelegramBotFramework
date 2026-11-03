using System;
using TelegramBotBase.Enums;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Args;

/// <summary>
///     Raised when a device session gets ended, either manually or by eviction.
///     The session has already been removed from the session list, but its forms are not disposed yet.
/// </summary>
public class SessionEndEventArgs : EventArgs
{
    public SessionEndEventArgs(long deviceId, IDeviceSession device, ESessionEndReason reason)
    {
        DeviceId = deviceId;
        Device = device;
        Reason = reason;
    }

    public long DeviceId { get; }

    public IDeviceSession Device { get; }

    public ESessionEndReason Reason { get; }
}
