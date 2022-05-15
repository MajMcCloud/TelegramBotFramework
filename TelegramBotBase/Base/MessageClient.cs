using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Extensions.Polling;

namespace TelegramBotBase.Base
{
    /// <summary>
    /// Base class for message handling
    /// </summary>
    public class MessageClient
    {


        public String APIKey { get; set; }

        public ITelegramBotClient TelegramClient { get; set; }

        private EventHandlerList __Events { get; set; } = new EventHandlerList();

        private static object __evOnMessageLoop = new object();

        private static object __evOnMessage = new object();

        private static object __evOnMessageEdit = new object();

        private static object __evCallbackQuery = new object();

        CancellationTokenSource __cancellationTokenSource;


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



        public MessageClient(String APIKey, Uri proxyUrl, NetworkCredential credential = null)
        {
            this.APIKey = APIKey;

            var proxy = new WebProxy(proxyUrl)
            {
                Credentials = credential
            };

            var httpClient = new HttpClient(
                new HttpClientHandler { Proxy = proxy, UseProxy = true }
            );

            this.TelegramClient = new Telegram.Bot.TelegramBotClient(APIKey, httpClient);

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

            var httpClient = new HttpClient(
                new HttpClientHandler { Proxy = proxy, UseProxy = true }
            );

            this.TelegramClient = new Telegram.Bot.TelegramBotClient(APIKey, httpClient);

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

        }



        public void StartReceiving()
        {
            __cancellationTokenSource = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { } // receive all update types
            };

            this.TelegramClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, __cancellationTokenSource.Token);
        }

        public void StopReceiving()
        {
            __cancellationTokenSource.Cancel();
        }


        public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            OnMessageLoop(new UpdateResult(update, null));

            return Task.CompletedTask;
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ApiRequestException exAPI)
            {
                Console.WriteLine($"Telegram API Error:\n[{exAPI.ErrorCode}]\n{exAPI.Message}");
            }
            else
            {
                Console.WriteLine(exception.ToString());
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// This will return the current list of bot commands.
        /// </summary>
        /// <returns></returns>
        public async Task<BotCommand[]> GetBotCommands(BotCommandScope scope = null, String languageCode = null)
        {
            return await this.TelegramClient.GetMyCommandsAsync(scope, languageCode);
        }


        /// <summary>
        /// This will set your bot commands to the given list.
        /// </summary>
        /// <param name="botcommands"></param>
        /// <returns></returns>
        public async Task SetBotCommands(List<BotCommand> botcommands, BotCommandScope scope = null, String languageCode = null)
        {
            await this.TelegramClient.SetMyCommandsAsync(botcommands, scope, languageCode);
        }

        /// <summary>
        /// This will delete the current list of bot commands.
        /// </summary>
        /// <returns></returns>
        public async Task DeleteBotCommands(BotCommandScope scope = null, String languageCode = null)
        {
            await this.TelegramClient.DeleteMyCommandsAsync(scope, languageCode);
        }


        #region "Events"



        public event Async.AsyncEventHandler<UpdateResult> MessageLoop
        {
            add
            {
                this.__Events.AddHandler(__evOnMessageLoop, value);
            }
            remove
            {
                this.__Events.RemoveHandler(__evOnMessageLoop, value);
            }
        }

        public void OnMessageLoop(UpdateResult update)
        {
            (this.__Events[__evOnMessageLoop] as Async.AsyncEventHandler<UpdateResult>)?.Invoke(this, update);
        }


        #endregion


    }
}
