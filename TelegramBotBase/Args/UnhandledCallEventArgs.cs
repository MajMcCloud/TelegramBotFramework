using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Args
{
    public class UnhandledCallEventArgs : EventArgs
    {
        public String Command { get; set; }

        public long DeviceId { get; set; }

        public DeviceSession Device {get;set;}

        public String RawData { get; set; }

        public int MessageId { get; set; }

        public Message Message { get; set; }

        public bool Handled { get; set; }


        public UnhandledCallEventArgs()
        {
            this.Handled = false;

        }

        public UnhandledCallEventArgs(String Command,String RawData, long DeviceId, int MessageId, Message message, DeviceSession Device) : this()
        {
            this.Command = Command;
            this.RawData = RawData;
            this.DeviceId = DeviceId;
            this.MessageId = MessageId;
            this.Message = message;
            this.Device = Device;
        }

    }
}
