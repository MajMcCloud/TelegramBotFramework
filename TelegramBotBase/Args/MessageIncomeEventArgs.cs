using System;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Base
{
    public class MessageIncomeEventArgs : EventArgs
    {

        public long DeviceId { get; set; }

        public DeviceSession Device { get; set; }

        public MessageResult Message { get; set; }

        public MessageIncomeEventArgs(long deviceId, DeviceSession device, MessageResult message)
        {
            this.DeviceId = deviceId;
            this.Device = device;
            Message = message;
        }




    }
}
