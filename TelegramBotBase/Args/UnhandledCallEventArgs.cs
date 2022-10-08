using System;
using Telegram.Bot.Types;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Args;

public class UnhandledCallEventArgs : EventArgs
{
    public UnhandledCallEventArgs()
    {
        Handled = false;
    }

    public UnhandledCallEventArgs(string command, string rawData, long deviceId, int messageId, Message message,
                                  DeviceSession device) : this()
    {
        Command = command;
        RawData = rawData;
        DeviceId = deviceId;
        MessageId = messageId;
        Message = message;
        Device = device;
    }

    public string Command { get; set; }

    public long DeviceId { get; set; }

    public DeviceSession Device { get; set; }

    public string RawData { get; set; }

    public int MessageId { get; set; }

    public Message Message { get; set; }

    public bool Handled { get; set; }
}