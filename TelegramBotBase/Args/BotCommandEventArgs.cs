using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Args
{
    /// <summary>
    /// Base class for given bot command results
    /// </summary>
    public class BotCommandEventArgs : EventArgs
    {
        public String Command { get; set; }

        public List<String> Parameters { get; set; }

        public long DeviceId { get; set; }

        public DeviceSession Device { get; set; }

        public bool Handled { get; set; } = false;

        public Message OriginalMessage { get; set; }


        public BotCommandEventArgs()
        {


        }

        public BotCommandEventArgs(String Command, List<String> Parameters, Message Message, long DeviceId, DeviceSession Device)
        {
            this.Command = Command;
            this.Parameters = Parameters;
            this.OriginalMessage = Message;
            this.DeviceId = DeviceId;
            this.Device = Device;
        }

    }
}
