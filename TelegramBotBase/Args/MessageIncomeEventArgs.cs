using System;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Base;

public class MessageIncomeEventArgs : EventArgs
{
    public MessageIncomeEventArgs(long deviceId, IDeviceSession device, MessageResult message)
    {
        DeviceId = deviceId;
        Device = device;
        Message = message;
    }

    public long DeviceId { get; set; }

    public IDeviceSession Device { get; set; }

    public MessageResult Message { get; set; }
}