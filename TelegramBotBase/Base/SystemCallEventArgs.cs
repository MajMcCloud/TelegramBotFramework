using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Base
{
    public class SystemCallEventArgs : EventArgs
    {
        public String Command { get; set; }

        public long DeviceId { get; set; }

        public DeviceSession Device {get;set;}


        public SystemCallEventArgs()
        {


        }

        public SystemCallEventArgs(String Command, long DeviceId, DeviceSession Device)
        {
            this.Command = Command;
            this.DeviceId = DeviceId;
            this.Device = Device;
        }

    }
}
