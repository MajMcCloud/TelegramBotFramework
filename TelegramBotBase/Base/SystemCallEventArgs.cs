using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Base
{
    /// <summary>
    /// Base class for given system call results
    /// </summary>
    public class SystemCallEventArgs : EventArgs
    {
        public String Command { get; set; }

        public List<String> Parameters { get; set; }

        public long DeviceId { get; set; }

        public DeviceSession Device { get; set; }

        public bool Handled { get; set; } = false;


        public SystemCallEventArgs()
        {


        }

        public SystemCallEventArgs(String Command, List<String> Parameters, long DeviceId, DeviceSession Device)
        {
            this.Command = Command;
            this.Parameters = Parameters;
            this.DeviceId = DeviceId;
            this.Device = Device;
        }

    }
}
