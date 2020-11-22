using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotBase.Args;
using TelegramBotBase.Attributes;
using TelegramBotBase.Base;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Sessions;

namespace TelegramBotBase
{
    /// <summary>
    /// Bot base class for full Device/Context and Messagehandling
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BotBase<T>
        where T : FormBase
    {
        public MessageClient Client { get; set; }

        /// <summary>
        /// Your TelegramBot APIKey
        /// </summary>
        public String APIKey { get; set; } = "";

        /// <summary>
        /// List of all running/active sessions
        /// </summary>
        public SessionBase<T> Sessions { get; set; }

        /// <summary>
        /// Contains System commands which will be available at everytime and didnt get passed to forms, i.e. /start
        /// </summary>
        public List<BotCommand> BotCommands { get; set; }


        #region "Events"

        private EventHandlerList __Events = new EventHandlerList();

        private static object __evSessionBegins = new object();

        private static object __evMessage = new object();

        private static object __evSystemCall = new object();

        public delegate Task BotCommandEventHandler(object sender, BotCommandEventArgs e);

        private static object __evException = new object();

        private static object __evUnhandledCall = new object();

        #endregion


        /// <summary>
        /// Enable the SessionState (you need to implement on call forms the IStateForm interface)
        /// </summary>
        public IStateMachine StateMachine { get; set; }


        public Dictionary<eSettings, uint> SystemSettings { get; private set; }

        private BotBase()
        {
            this.SystemSettings = new Dictionary<eSettings, uint>();

            SetSetting(eSettings.NavigationMaximum, 10);
            SetSetting(eSettings.LogAllMessages, false);
            SetSetting(eSettings.SkipAllMessages, false);
            SetSetting(eSettings.SaveSessionsOnConsoleExit, false);

            this.BotCommands = new List<BotCommand>();

            this.Sessions = new SessionBase<T>();
            this.Sessions.BotBase = this;
        }

        /// <summary>
        /// Simple start of your Bot with the APIKey
        /// </summary>
        /// <param name="apiKey"></param>
        public BotBase(String apiKey, bool initClient = true) : this()
        {
            this.APIKey = apiKey;

            if (!initClient)
                return;

            this.Client = new Base.MessageClient(this.APIKey);
            this.Client.TelegramClient.Timeout = new TimeSpan(0, 1, 0);

            this.Sessions.Client = this.Client;
        }

        /// <summary>
        /// Simple start of your Bot with the APIKey and a proxyAdress
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="proxyBaseAddress">i.e. https://127.0.0.1:10000</param>
        public BotBase(String apiKey, System.Net.Http.HttpClient proxy) : this(apiKey, false)
        {
            this.Client = new Base.MessageClient(this.APIKey, proxy);

            this.Sessions.Client = this.Client;
        }

        /// <summary>
        /// Simple start of your Bot with the APIKey and a TelegramBotClient instance.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="client"></param>
        public BotBase(String apiKey, TelegramBotClient client) : this(apiKey, false)
        {
            this.Client = new Base.MessageClient(this.APIKey, client);

            this.Sessions.Client = this.Client;
        }

        /// <summary>
        /// Simple start of your Bot with the APIKey and a proxyAdress
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="proxyBaseAddress">i.e. https://127.0.0.1:10000</param>
        public BotBase(String apiKey, String proxyBaseAddress) : this(apiKey, false)
        {
            var url = new Uri(proxyBaseAddress);

            this.Client = new Base.MessageClient(this.APIKey, url);

            this.Sessions.Client = this.Client;
        }

        /// <summary>
        /// Simple start of your Bot with the APIKey and a proxyAdress
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="proxyHost">i.e. 127.0.0.1</param>
        /// <param name="proxyPort">i.e. 10000</param>
        public BotBase(String apiKey, String proxyHost, int proxyPort) : this(apiKey, false)
        {
            this.Client = new Base.MessageClient(this.APIKey, proxyHost, proxyPort);

            this.Sessions.Client = this.Client;
        }

        /// <summary>
        /// Start your Bot
        /// </summary>
        public void Start()
        {
            if (this.Client == null)
                return;

            this.Client.Message += Client_Message;
            this.Client.MessageEdit += Client_MessageEdit;
            this.Client.Action += Client_Action;

            if (this.StateMachine != null)
            {
                this.Sessions.LoadSessionStates(this.StateMachine);
            }

            //Enable auto session saving
            if (this.GetSetting(eSettings.SaveSessionsOnConsoleExit, false))
            {
                TelegramBotBase.Tools.Console.SetHandler(() =>
                {
                    this.Sessions.SaveSessionStates(this.StateMachine);
                });
            }

            this.Client.TelegramClient.StartReceiving();
        }


        /// <summary>
        /// Stop your Bot
        /// </summary>
        public void Stop()
        {
            if (this.Client == null)
                return;

            this.Client.Message -= Client_Message;
            this.Client.Action -= Client_Action;

            this.Client.TelegramClient.StopReceiving();

            this.Sessions.SaveSessionStates();
        }

        /// <summary>
        /// Send a message to all active Sessions.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SentToAll(String message)
        {
            if (this.Client == null)
                return;

            foreach (var s in this.Sessions.SessionList)
            {
                await this.Client.TelegramClient.SendTextMessageAsync(s.Key, message);
            }
        }

        private async void Client_Message(object sender, MessageResult e)
        {
            if (this.GetSetting(eSettings.SkipAllMessages, false))
                return;

            try
            {
                DeviceSession ds = this.Sessions.GetSession(e.DeviceId);
                e.Device = ds;

                if (this.GetSetting(eSettings.LogAllMessages, false))
                {
                    OnMessage(new MessageIncomeEventArgs(e.DeviceId, ds, e));
                }

                ds?.OnMessageReceived(new MessageReceivedEventArgs(e.Message));

                await Client_TryMessage(sender, e);
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {

            }
            catch (Exception ex)
            {
                DeviceSession ds = this.Sessions.GetSession(e.DeviceId);
                OnException(new SystemExceptionEventArgs(e.Message.Text, ds?.DeviceId ?? -1, ds, ex));
            }
        }

        private async Task Client_TryMessage(object sender, MessageResult e)
        {
            DeviceSession ds = e.Device;
            if (ds == null)
            {
                ds = await this.Sessions.StartSession<T>(e.DeviceId);
                e.Device = ds;

                ds.LastMessage = e.Message;

                OnSessionBegins(new SessionBeginEventArgs(e.DeviceId, ds));
            }

            ds.LastAction = DateTime.Now;
            ds.LastMessage = e.Message;

            //Is this a bot command ?
            if (e.IsBotCommand && this.BotCommands.Count(a => "/" + a.Command == e.BotCommand) > 0)
            {
                var sce = new BotCommandEventArgs(e.BotCommand, e.BotCommandParameters, e.Message, ds.DeviceId, ds);
                await OnBotCommand(sce);

                if (sce.Handled)
                    return;
            }

            FormBase activeForm = null;

            int i = 0;

            //Should formulars get navigated (allow maximum of 10, to dont get loops)
            do
            {
                i++;

                //Reset navigation
                ds.FormSwitched = false;

                activeForm = ds.ActiveForm;

                //Pre Loading Event
                await activeForm.PreLoad(e);

                //Send Load event to controls
                await activeForm.LoadControls(e);

                //Loading Event
                await activeForm.Load(e);

                //Is Attachment ? (Photo, Audio, Video, Contact, Location, Document)
                if (e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Contact | e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Document | e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Location |
                    e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Photo | e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Video | e.Message.Type == Telegram.Bot.Types.Enums.MessageType.Audio)
                {
                    await activeForm.SentData(new DataResult(e));
                }

                //Render Event
                if (!ds.FormSwitched)
                {
                    await activeForm.RenderControls(e);

                    await activeForm.Render(e);
                }

                e.IsFirstHandler = false;

            } while (ds.FormSwitched && i < this.GetSetting(eSettings.NavigationMaximum, 10));


        }

        private async void Client_MessageEdit(object sender, MessageResult e)
        {
            if (this.GetSetting(eSettings.SkipAllMessages, false))
                return;

            try
            {
                DeviceSession ds = this.Sessions.GetSession(e.DeviceId);
                e.Device = ds;

                if (this.GetSetting(eSettings.LogAllMessages, false))
                {
                    OnMessage(new MessageIncomeEventArgs(e.DeviceId, ds, e));
                }

                //Call same, to handle received liked edited
                ds?.OnMessageReceived(new MessageReceivedEventArgs(e.Message));

                await Client_TryMessageEdit(sender, e);
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {

            }
            catch (Exception ex)
            {
                DeviceSession ds = this.Sessions.GetSession(e.DeviceId);
                OnException(new SystemExceptionEventArgs(e.Message.Text, ds?.DeviceId ?? -1, ds, ex));
            }
        }

        private async Task Client_TryMessageEdit(object sender, MessageResult e)
        {
            DeviceSession ds = e.Device;
            if (ds == null)
            {
                ds = await this.Sessions.StartSession<T>(e.DeviceId);
                e.Device = ds;
            }

            ds.LastAction = DateTime.Now;
            ds.LastMessage = e.Message;

            //Pre Loading Event
            await ds.ActiveForm.Edited(e);

            //When form has been switched due navigation within the edit method, reopen Client_Message
            if (ds.FormSwitched)
            {
                await Client_TryMessage(sender, e);
            }

        }

        private void Client_Action(object sender, MessageResult e)
        {
            try
            {
                DeviceSession ds = this.Sessions.GetSession(e.DeviceId);
                e.Device = ds;

                if (this.GetSetting(eSettings.LogAllMessages, false))
                {
                    OnMessage(new MessageIncomeEventArgs(e.DeviceId, ds, e));
                }

                Client_TryAction(sender, e);
            }
            catch (Exception ex)
            {
                DeviceSession ds = this.Sessions.GetSession(e.DeviceId);
                OnException(new SystemExceptionEventArgs(e.Message.Text, ds?.DeviceId ?? -1, ds, ex));
            }
        }

        private async void Client_TryAction(object sender, MessageResult e)
        {
            DeviceSession ds = e.Device;
            if (ds == null)
            {
                ds = await this.Sessions.StartSession<T>(e.DeviceId);
                e.Device = ds;
            }

            ds.LastAction = DateTime.Now;
            ds.LastMessage = e.Message;

            FormBase activeForm = null;

            int i = 0;

            //Should formulars get navigated (allow maximum of 10, to dont get loops)
            do
            {
                i++;

                //Reset navigation
                ds.FormSwitched = false;

                activeForm = ds.ActiveForm;

                //Pre Loading Event
                await activeForm.PreLoad(e);

                //Send Load event to controls
                await activeForm.LoadControls(e);

                //Loading Event
                await activeForm.Load(e);

                //Action Event
                if (!ds.FormSwitched)
                {
                    //Send Action event to controls
                    await activeForm.ActionControls(e);

                    //Send Action event to form itself
                    await activeForm.Action(e);

                    if (!e.Handled)
                    {
                        var uhc = new UnhandledCallEventArgs(e.Message.Text, e.RawData, ds.DeviceId, e.MessageId, e.Message, ds);
                        OnUnhandledCall(uhc);

                        if (uhc.Handled)
                        {
                            e.Handled = true;
                            if (!ds.FormSwitched)
                            {
                                break;
                            }
                        }
                    }

                }

                //Render Event
                if (!ds.FormSwitched)
                {
                    await activeForm.RenderControls(e);

                    await activeForm.Render(e);
                }

                e.IsFirstHandler = false;

            } while (ds.FormSwitched && i < this.GetSetting(eSettings.NavigationMaximum, 10));

        }

        /// <summary>
        /// This method will update all local created bot commands to the botfather.
        /// </summary>
        public async Task UploadBotCommands()
        {
            await this.Client.SetBotCommands(this.BotCommands);
        }

        /// <summary>
        /// Could set a variety of settings to improve the bot handling.
        /// </summary>
        /// <param name="set"></param>
        /// <param name="Value"></param>
        public void SetSetting(eSettings set, uint Value)
        {
            this.SystemSettings[set] = Value;
        }

        /// <summary>
        /// Could set a variety of settings to improve the bot handling.
        /// </summary>
        /// <param name="set"></param>
        /// <param name="Value"></param>
        public void SetSetting(eSettings set, bool Value)
        {
            this.SystemSettings[set] = (Value ? 1u : 0u);
        }

        /// <summary>
        /// Could get the current value of a setting
        /// </summary>
        /// <param name="set"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public uint GetSetting(eSettings set, uint defaultValue)
        {
            if (!this.SystemSettings.ContainsKey(set))
                return defaultValue;

            return this.SystemSettings[set];
        }

        /// <summary>
        /// Could get the current value of a setting
        /// </summary>
        /// <param name="set"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public bool GetSetting(eSettings set, bool defaultValue)
        {
            if (!this.SystemSettings.ContainsKey(set))
                return defaultValue;

            return this.SystemSettings[set] == 0u ? false : true;
        }

        #region "Events"

        /// <summary>
        /// Will be called if a session/context gets started
        /// </summary>

        public event EventHandler<SessionBeginEventArgs> SessionBegins
        {
            add
            {
                this.__Events.AddHandler(__evSessionBegins, value);
            }
            remove
            {
                this.__Events.RemoveHandler(__evSessionBegins, value);
            }
        }

        public void OnSessionBegins(SessionBeginEventArgs e)
        {
            (this.__Events[__evSessionBegins] as EventHandler<SessionBeginEventArgs>)?.Invoke(this, e);

        }

        /// <summary>
        /// Will be called on incomming message
        /// </summary>
        public event EventHandler<MessageIncomeEventArgs> Message
        {
            add
            {
                this.__Events.AddHandler(__evMessage, value);
            }
            remove
            {
                this.__Events.RemoveHandler(__evMessage, value);
            }
        }

        public void OnMessage(MessageIncomeEventArgs e)
        {
            (this.__Events[__evMessage] as EventHandler<MessageIncomeEventArgs>)?.Invoke(this, e);

        }

        /// <summary>
        /// Will be called if a bot command gets raised
        /// </summary>
        public event BotCommandEventHandler BotCommand;


        public async Task OnBotCommand(BotCommandEventArgs e)
        {
            if (this.BotCommand != null)
                await BotCommand(this, e);
        }

        /// <summary>
        /// Will be called on an inner exception
        /// </summary>
        public event EventHandler<SystemExceptionEventArgs> Exception
        {
            add
            {
                this.__Events.AddHandler(__evException, value);
            }
            remove
            {
                this.__Events.RemoveHandler(__evException, value);
            }
        }

        public void OnException(SystemExceptionEventArgs e)
        {
            (this.__Events[__evException] as EventHandler<SystemExceptionEventArgs>)?.Invoke(this, e);

        }

        /// <summary>
        /// Will be called if no form handeled this call
        /// </summary>
        public event EventHandler<UnhandledCallEventArgs> UnhandledCall
        {
            add
            {
                this.__Events.AddHandler(__evUnhandledCall, value);
            }
            remove
            {
                this.__Events.RemoveHandler(__evUnhandledCall, value);
            }
        }

        public void OnUnhandledCall(UnhandledCallEventArgs e)
        {
            (this.__Events[__evUnhandledCall] as EventHandler<UnhandledCallEventArgs>)?.Invoke(this, e);

        }

        #endregion

    }
}
