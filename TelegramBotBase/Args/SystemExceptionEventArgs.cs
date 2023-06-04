using System;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Args;

public class SystemExceptionEventArgs : EventArgs
{
    public SystemExceptionEventArgs()
    {
    }

    public SystemExceptionEventArgs(string command, long deviceId, DeviceSession device, Exception error)
    {
        Command = command;
        DeviceId = deviceId;
        Device = device;
        Error = error;
    }

    public string Command { get; set; }

    public long DeviceId { get; set; }

    public DeviceSession Device { get; set; }

    public Exception Error { get; set; }
}