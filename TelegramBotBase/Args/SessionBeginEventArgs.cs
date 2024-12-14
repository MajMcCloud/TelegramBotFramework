using System;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Base;

public class SessionBeginEventArgs : EventArgs
{
    public SessionBeginEventArgs(long deviceId, IDeviceSession device)
    {
        DeviceId = deviceId;
        Device = device;
    }

    public long DeviceId { get; set; }

    public IDeviceSession Device { get; set; }
}