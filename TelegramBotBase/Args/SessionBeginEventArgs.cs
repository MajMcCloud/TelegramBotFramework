using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Base
{
    public class SessionBeginEventArgs : EventArgs
    {
        public long DeviceId { get; set; }

        public DeviceSession Device { get; set; }

        public SessionBeginEventArgs(long DeviceId, DeviceSession Device)
        {
            this.DeviceId = DeviceId;
            this.Device = Device;
        }
    }
}
