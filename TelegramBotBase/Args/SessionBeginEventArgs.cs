using System;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Base
{
    public class SessionBeginEventArgs : EventArgs
    {
        public long DeviceId { get; set; }

        public DeviceSession Device { get; set; }

        public SessionBeginEventArgs(long deviceId, DeviceSession device)
        {
            this.DeviceId = deviceId;
            this.Device = device;
        }
    }
}
