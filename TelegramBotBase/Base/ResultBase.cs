using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotBase.Base
{
    public class ResultBase : EventArgs
    {
        public MessageClient Client { get; set; }

        public virtual long DeviceId { get; set; }

        public int MessageId
        {
            get
            {
                return this.Message.MessageId;
            }
        }

        public Telegram.Bot.Types.Message Message { get; set; }

        /// <summary>
        /// Deletes the current message
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public virtual async Task DeleteMessage()
        {
            await DeleteMessage(this.MessageId);
        }

        /// <summary>
        ///Deletes the current message or the given one.
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public virtual async Task DeleteMessage(int messageId = -1)
        {
            try
            {
                await this.Client.TelegramClient.DeleteMessageAsync(this.DeviceId, (messageId == -1 ? this.MessageId : messageId));
            }
            catch
            {

            }
        }

    }
}
