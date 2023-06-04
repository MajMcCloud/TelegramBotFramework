using System;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Base;

public class SessionBeginEventArgs : EventArgs
{
    public SessionBeginEventArgs(long deviceId, DeviceSession device)
    {
        DeviceId = deviceId;
        Device = device;
    }

    public long DeviceId { get; set; }

    public DeviceSession Device { get; set; }
}