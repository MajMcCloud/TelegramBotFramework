using System;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Args;

public class SystemExceptionEventArgs : EventArgs
{
    public SystemExceptionEventArgs()
    {
    }

    public SystemExceptionEventArgs(string command, long deviceId, IDeviceSession device, Exception error)
    {
        Command = command;
        DeviceId = deviceId;
        Device = device;
        Error = error;
    }

    public string Command { get; set; }

    public long DeviceId { get; set; }

    public IDeviceSession Device { get; set; }

    public Exception Error { get; set; }
}