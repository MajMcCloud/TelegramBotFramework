using System;
using System.Collections.Generic;
using Telegram.Bot.Types;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Args
{
    /// <summary>
    /// Base class for given bot command results
    /// </summary>
    public class BotCommandEventArgs : EventArgs
    {
        public string Command { get; set; }

        public List<string> Parameters { get; set; }

        public long DeviceId { get; set; }

        public DeviceSession Device { get; set; }

        public bool Handled { get; set; } = false;

        public Message OriginalMessage { get; set; }


        public BotCommandEventArgs()
        {


        }

        public BotCommandEventArgs(string command, List<string> parameters, Message message, long deviceId, DeviceSession device)
        {
            this.Command = command;
            this.Parameters = parameters;
            OriginalMessage = message;
            this.DeviceId = deviceId;
            this.Device = device;
        }

    }
}
