using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramBotBase.Base
{
    /// <summary>
    /// Base class for message handling
    /// </summary>
    public class MessageClient
    {


        public String APIKey { get; set; }

        public Telegram.Bot.TelegramBotClient TelegramClient { get; set; }

        private EventHandlerList __Events { get; set; } = new EventHandlerList();

        private static object __evOnMessage = new object();

        private static object __evOnMessageEdit = new object();

        private static object __evCallbackQuery = new object();


        public MessageClient(String APIKey)
        {
            this.APIKey = APIKey;
            this.TelegramClient = new Telegram.Bot.TelegramBotClient(APIKey);

            Prepare();
        }

        public MessageClient(String APIKey, HttpClient proxy)
        {
            this.APIKey = APIKey;
            this.TelegramClient = new Telegram.Bot.TelegramBotClient(APIKey, proxy);


            Prepare();
        }

        public MessageClient(String APIKey, Uri proxyUrl)
        {
            this.APIKey = APIKey;

            var proxy = new WebProxy(proxyUrl);

            this.TelegramClient = new Telegram.Bot.TelegramBotClient(APIKey, proxy);

            Prepare();
        }

        /// <summary>
        /// Initializes the client with a proxy
        /// </summary>
        /// <param name="APIKey"></param>
        /// <param name="proxyHost">i.e. 127.0.0.1</param>
        /// <param name="proxyPort">i.e. 10000</param>
        public MessageClient(String APIKey, String proxyHost, int proxyPort)
        {
            this.APIKey = APIKey;

            var proxy = new WebProxy(proxyHost, proxyPort);

            this.TelegramClient = new Telegram.Bot.TelegramBotClient(APIKey, proxy);

            Prepare();
        }

        public MessageClient(String APIKey, Telegram.Bot.TelegramBotClient Client)
        {
            this.APIKey = APIKey;
            this.TelegramClient = Client;

            Prepare();
        }


        public void Prepare()
        {
            this.TelegramClient.Timeout = new TimeSpan(0, 0, 30);

            this.TelegramClient.OnMessage += TelegramClient_OnMessage;
            this.TelegramClient.OnMessageEdited += TelegramClient_OnMessageEdited;
            this.TelegramClient.OnCallbackQuery += TelegramClient_OnCallbackQuery;
        }

        private async void TelegramClient_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            //Skip empty messages by default
            if (e.Message == null)
                return;

            try
            {
                var mr = new MessageResult(e);
                mr.Client = this;
                OnMessage(mr);
            }
            catch
            {

            }
        }


        private async void TelegramClient_OnMessageEdited(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            //Skip empty messages by default
            if (e.Message == null)
                return;

            try
            {
                var mr = new MessageResult(e);
                mr.Client = this;
                OnMessageEdit(mr);
            }
            catch
            {

            }
        }

        private async void TelegramClient_OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            try
            {
                var ar = new MessageResult(e);
                ar.Client = this;
                OnAction(ar);
            }
            catch
            {

            }
        }

        /// <summary>
        /// This will return the current list of bot commands.
        /// </summary>
        /// <returns></returns>
        public async Task<BotCommand[]> GetBotCommands()
        {
            return await this.TelegramClient.GetMyCommandsAsync();
        }

        /// <summary>
        /// This will set your bot commands to the given list.
        /// </summary>
        /// <param name="botcommands"></param>
        /// <returns></returns>
        public async Task SetBotCommands(List<BotCommand> botcommands)
        {
            await this.TelegramClient.SetMyCommandsAsync(botcommands);
        }




        #region "Events"

        public event EventHandler<MessageResult> Message
        {
            add
            {
                this.__Events.AddHandler(__evOnMessage, value);
            }
            remove
            {
                this.__Events.RemoveHandler(__evOnMessage, value);
            }
        }

        public void OnMessage(MessageResult result)
        {
            (this.__Events[__evOnMessage] as EventHandler<MessageResult>)?.Invoke(this, result);
        }

        public event EventHandler<MessageResult> MessageEdit
        {
            add
            {
                this.__Events.AddHandler(__evOnMessageEdit, value);
            }
            remove
            {
                this.__Events.RemoveHandler(__evOnMessageEdit, value);
            }
        }

        public void OnMessageEdit(MessageResult result)
        {
            (this.__Events[__evOnMessageEdit] as EventHandler<MessageResult>)?.Invoke(this, result);
        }

        public event EventHandler<MessageResult> Action
        {
            add
            {
                this.__Events.AddHandler(__evCallbackQuery, value);
            }
            remove
            {
                this.__Events.RemoveHandler(__evCallbackQuery, value);
            }
        }

        public void OnAction(MessageResult result)
        {
            (this.__Events[__evCallbackQuery] as EventHandler<MessageResult>)?.Invoke(this, result);
        }


        #endregion


    }
}
