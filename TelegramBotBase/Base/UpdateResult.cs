using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegram.Bot.Types;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Base
{
    public class UpdateResult : ResultBase
    {
        public UpdateResult(Update rawData, DeviceSession device)
        {
            RawData = rawData;
            Device = device;

            
        }

        /// <summary>
        /// Returns the Device/ChatId
        /// </summary>
        public override long DeviceId
        {
            get
            {
                return this.RawData?.Message?.Chat?.Id
                    ?? this.RawData?.CallbackQuery?.Message?.Chat?.Id
                    ?? Device?.DeviceId
                    ?? 0;
            }
        }

        public Update RawData { get; set; }

        public override Message Message
        {
            get
            {
                return RawData?.Message
                    ?? RawData?.EditedMessage
                    ?? RawData?.ChannelPost
                    ?? RawData?.EditedChannelPost
                    ?? RawData?.CallbackQuery?.Message;
            }
        }
        


        public DeviceSession Device
        {
            get;
            set;
        }


    }

}
