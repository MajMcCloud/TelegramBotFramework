using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Args;
using TelegramBotBase.Attributes;
using TelegramBotBase.Base;
using TelegramBotBase.Enums;
using TelegramBotBase.Factories.MessageLoops;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Sessions;

namespace TelegramBotBase
{
    /// <summary>
    /// Bot base class for full Device/Context and Messagehandling
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BotBase
    {
        public MessageClient Client { get; set; }

        /// <summary>
        /// Your TelegramBot APIKey
        /// </summary>
        public String APIKey { get; set; } = "";

        /// <summary>
        /// List of all running/active sessions
        /// </summary>
        public SessionBase Sessions { get; set; }

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

        /// <summary>
        /// Offers functionality to manage the creation process of the start form.
        /// </summary>
        public IStartFormFactory StartFormFactory { get; set; }

        /// <summary>
        /// Contains the message loop factory, which cares about "message-management."
        /// </summary>
        public IMessageLoopFactory MessageLoopFactory { get; set; }

        /// <summary>
        /// All internal used settings.
        /// </summary>
        public Dictionary<eSettings, uint> SystemSettings { get; private set; }

        public BotBase()
        {
            this.SystemSettings = new Dictionary<eSettings, uint>();

            SetSetting(eSettings.MaxNumberOfRetries, 5);
            SetSetting(eSettings.NavigationMaximum, 10);
            SetSetting(eSettings.LogAllMessages, false);
            SetSetting(eSettings.SkipAllMessages, false);
            SetSetting(eSettings.SaveSessionsOnConsoleExit, false);

            this.BotCommands = new List<BotCommand>();

            this.Sessions = new SessionBase();
            this.Sessions.BotBase = this;
        }



        /// <summary>
        /// Start your Bot
        /// </summary>
        public void Start()
        {
            if (this.Client == null)
                return;

            this.Client.MessageLoop += Client_MessageLoop;


            if (this.StateMachine != null)
            {
                this.Sessions.LoadSessionStates(this.StateMachine);
            }

            //Enable auto session saving
            if (this.GetSetting(eSettings.SaveSessionsOnConsoleExit, false))
            {
                TelegramBotBase.Tools.Console.SetHandler(() =>
                {
                    this.Sessions.SaveSessionStates();
                });
            }

            DeviceSession.MaxNumberOfRetries = this.GetSetting(eSettings.MaxNumberOfRetries, 5);

            this.Client.StartReceiving();
        }


        private async Task Client_MessageLoop(object sender, UpdateResult e)
        {
            DeviceSession ds = this.Sessions.GetSession(e.DeviceId);
            if (ds == null)
            {
                ds = this.Sessions.StartSession(e.DeviceId).GetAwaiter().GetResult();
                e.Device = ds;
                ds.LastMessage = e.RawData.Message;

                OnSessionBegins(new SessionBeginEventArgs(e.DeviceId, ds));
            }

            var mr = new MessageResult(e.RawData);

            int i = 0;

            //Should formulars get navigated (allow maximum of 10, to dont get loops)
            do
            {
                i++;

                //Reset navigation
                ds.FormSwitched = false;

                await MessageLoopFactory.MessageLoop(this, ds, e, mr);

                mr.IsFirstHandler = false;

            } while (ds.FormSwitched && i < this.GetSetting(eSettings.NavigationMaximum, 10));
        }


        /// <summary>
        /// Stop your Bot
        /// </summary>
        public void Stop()
        {
            if (this.Client == null)
                return;

            this.Client.MessageLoop -= Client_MessageLoop;


            this.Client.StopReceiving();

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



        /// <summary>
        /// This will invoke the full message loop for the device even when no "userevent" like message or action has been raised.
        /// </summary>
        /// <param name="DeviceId">Contains the device/chat id of the device to update.</param>
        public async Task InvokeMessageLoop(long DeviceId)
        {
            var mr = new MessageResult();

            await InvokeMessageLoop(DeviceId, mr);
        }

        /// <summary>
        /// This will invoke the full message loop for the device even when no "userevent" like message or action has been raised.
        /// </summary>
        /// <param name="DeviceId">Contains the device/chat id of the device to update.</param>
        /// <param name="e"></param>
        public async Task InvokeMessageLoop(long DeviceId, MessageResult e)
        {
            try
            {
                DeviceSession ds = this.Sessions.GetSession(DeviceId);
                e.Device = ds;

                await MessageLoopFactory.MessageLoop(this, ds, new UpdateResult(e.UpdateData, ds), e);
                //await Client_Loop(this, e);
            }
            catch (Exception ex)
            {
                DeviceSession ds = this.Sessions.GetSession(DeviceId);
                OnException(new SystemExceptionEventArgs(e.Message.Text, DeviceId, ds, ex));
            }
        }


        /// <summary>
        /// Will get invoke on an unhandled call.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MessageLoopFactory_UnhandledCall(object sender, UnhandledCallEventArgs e)
        {
            OnUnhandledCall(e);
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
