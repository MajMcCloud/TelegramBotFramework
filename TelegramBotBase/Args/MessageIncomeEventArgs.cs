using System;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Base;

public class MessageIncomeEventArgs : EventArgs
{
    public MessageIncomeEventArgs(long deviceId, DeviceSession device, MessageResult message)
    {
        DeviceId = deviceId;
        Device = device;
        Message = message;
    }

    public long DeviceId { get; set; }

    public DeviceSession Device { get; set; }

    public MessageResult Message { get; set; }
}