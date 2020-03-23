using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Args
{
    public class SystemExceptionEventArgs : EventArgs
    {

        public String Command { get; set; }

        public long DeviceId { get; set; }

        public DeviceSession Device { get; set; }

        public Exception Error { get; set; }

        public SystemExceptionEventArgs()
        {


        }

        public SystemExceptionEventArgs(String Command, long DeviceId, DeviceSession Device, Exception error)
        {
            this.Command = Command;
            this.DeviceId = DeviceId;
            this.Device = Device;
            this.Error = error;
        }
    }

}
