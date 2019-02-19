using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotBase.Base
{
    public class MessageResult : ResultBase
    {
        public Telegram.Bot.Args.MessageEventArgs RawMessageData { get; set; }

        public Telegram.Bot.Args.CallbackQueryEventArgs RawCallbackData { get; set; }

        public override long DeviceId
        {
            get
            {
                return this.RawMessageData?.Message?.Chat.Id ?? this.RawCallbackData?.CallbackQuery.Message?.Chat.Id ?? 0;
            }
        }

        /// <summary>
        /// Die Id der Nachricht
        /// </summary>
        public new int MessageId
        {
            get
            {
                return this.Message?.MessageId ?? this.RawCallbackData?.CallbackQuery?.Message?.MessageId ?? 0;
            }
        }

        public String Command
        {
            get
            {
                return this.RawMessageData?.Message?.Text ?? "";
            }
        }

        public String MessageText
        {
            get
            {
                return this.RawMessageData?.Message?.Text ?? "";
            }
        }

        /// <summary>
        /// Ist diese eine Aktion? (z.B.: Button Klick)
        /// </summary>
        public bool IsAction
        {
            get
            {
                return (this.RawCallbackData != null);
            }
        }

        public bool Handled { get; set; } = false;

        public String RawData
        {
            get
            {
                return this.RawCallbackData?.CallbackQuery?.Data;
            }
        }

        public T GetData<T>()
            where T : class
        {
            T cd = null;
            try
            {
                cd = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(this.RawData);

                return cd;
            }
            catch
            {

            }

            return null;
        }

        /// <summary>
        /// Bestätigt den Erhalt der Aktion.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task ConfirmAction(String message = "")
        {
            try
            {
                await this.Client.TelegramClient.AnswerCallbackQueryAsync(this.RawCallbackData.CallbackQuery.Id, message);
            }
            catch
            {

            }
        }

        public override async Task DeleteMessage()
        {
            try
            {
                await base.DeleteMessage(this.MessageId);
            }
            catch
            {

            }
        }


        public MessageResult(Telegram.Bot.Args.MessageEventArgs rawdata)
        {
            this.RawMessageData = rawdata;
            this.Message = rawdata.Message;
        }

        public MessageResult(Telegram.Bot.Args.CallbackQueryEventArgs callback)
        {
            this.RawCallbackData = callback;
            this.Message = callback.CallbackQuery.Message;
        }

    }
}
