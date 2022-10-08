using System;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Args
{
    public class SystemExceptionEventArgs : EventArgs
    {

        public string Command { get; set; }

        public long DeviceId { get; set; }

        public DeviceSession Device { get; set; }

        public Exception Error { get; set; }

        public SystemExceptionEventArgs()
        {


        }

        public SystemExceptionEventArgs(string command, long deviceId, DeviceSession device, Exception error)
        {
            this.Command = command;
            this.DeviceId = deviceId;
            this.Device = device;
            Error = error;
        }
    }

}
