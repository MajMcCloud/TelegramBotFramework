using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
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
        public SessionBase Sessions { get; set; }

        /// <summary>
        /// Contains System commands which will be available at everytime and didnt get passed to forms, i.e. /start
        /// </summary>
        public List<String> SystemCalls { get; set; }

        private EventHandlerList __Events = new EventHandlerList();

        private static object __evSessionBegins = new object();

        private static object __evMessage = new object();

        private static object __evSystemCall = new object();

        private static object __evException = new object();

        private static object __evUnhandledCall = new object();


        /// <summary>
        /// Skips all messages during running (good for big delay updates)
        /// </summary>
        public bool SkipAllMessages { get; set; } = false;

        /// <summary>
        /// Loggs all messages and sent them to the event handler
        /// </summary>
        public bool LogAllMessages { get; set; } = false;

        /// <summary>
        /// Simple start of your Bot with the APIKey
        /// </summary>
        /// <param name="apiKey"></param>
        public BotBase(String apiKey)
        {
            this.APIKey = apiKey;

            this.Client = new Base.MessageClient(this.APIKey);

            this.SystemCalls = new List<string>();

            this.Sessions = new SessionBase();
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
            this.Client.Action += Client_Action;


            this.Client.TelegramClient.StartReceiving();
        }

        /// <summary>
        /// Stop your Bot
        /// </summary>
        public void Stop()
        {
            if (this.Client == null)
                return;

            this.Client.TelegramClient.StopReceiving();
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

        private void Client_Message(object sender, MessageResult e)
        {
            if (this.SkipAllMessages)
                return;

            try
            {
                if (LogAllMessages)
                {
                    DeviceSession ds2 = this.Sessions.GetSession(e.DeviceId);
                    OnMessage(new MessageIncomeResult(e.DeviceId, ds2, e));
                }


                Client_TryMessage(sender, e);
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

        private async void Client_TryMessage(object sender, MessageResult e)
        {
            DeviceSession ds = this.Sessions.GetSession(e.DeviceId);
            if (ds == null)
            {
                ds = await this.Sessions.StartSession<T>(e.DeviceId);

                ds.LastMessage = e.MessageId;

                OnSessionBegins(new SessionBeginResult(e.DeviceId, ds));
            }

            ds.LastAction = DateTime.Now;
            ds.LastMessage = e.MessageId;


            //Ist das ein Systembefehl ?
            if (e.Message.Text != null && this.SystemCalls.Contains(e.Message.Text))
            {
                var sce = new SystemCallEventArgs(e.Message.Text, ds.DeviceId, ds);
                OnSystemCall(sce);
                //return;
            }

            FormBase activeForm = null;

            int i = 0;

            //Sollten die Formulare gewechselt werden, alle durchgehen (maximal 10 Versuche, um Schleifen zu verhindern)
            do
            {
                i++;

                activeForm = ds.ActiveForm;

                //Wenn das Formular sich selbst um die Events kümmert, nicht weiter machen
                if (activeForm.CustomEventManagement)
                    return;

                //Pre Loading Event
                await activeForm.PreLoad(e);

                //Loading Event
                await activeForm.Load(e);

                ////Action Event
                //if (!activeForm.FormSwitched)
                //    await activeForm.Action(e);

                //Render Event
                if (!activeForm.FormSwitched)
                    await activeForm.Render(e);

            } while (activeForm.FormSwitched && i < 10);

        }

        private void Client_Action(object sender, MessageResult e)
        {
            try
            {
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
            DeviceSession ds = this.Sessions.GetSession(e.DeviceId);
            if (ds == null)
            {
                ds = await this.Sessions.StartSession<T>(e.DeviceId);
            }


            ds.LastAction = DateTime.Now;
            ds.LastMessage = e.MessageId;

            FormBase activeForm = null;

            int i = 0;

            //Sollten die Formulare gewechselt werden, alle durchgehen (maximal 10 Versuche, um Schleifen zu verhindern)
            do
            {
                i++;

                activeForm = ds.ActiveForm;

                //Wenn das Formular sich selbst um die Events kümmert, nicht weiter machen
                if (activeForm.CustomEventManagement)
                    return;

                //Pre Loading Event
                await activeForm.PreLoad(e);

                //Loading Event
                await activeForm.Load(e);

                //Action Event
                if (!activeForm.FormSwitched)
                {
                    await activeForm.Action(e);

                    if (!e.Handled)
                    {
                        var uhc = new UnhandledCallEventArgs(e.Message.Text, e.RawData, ds.DeviceId, e.MessageId, e.Message, ds);
                        OnUnhandledCall(uhc);

                        if (uhc.Handled)
                        {
                            if (activeForm.FormSwitched)
                            {
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                }

                //Render Event
                if (!activeForm.FormSwitched)
                    await activeForm.Render(e);

            } while (activeForm.FormSwitched && i < 10);

        }

        /// <summary>
        /// Will be called if a session/context gets started
        /// </summary>

        public event EventHandler<SessionBeginResult> SessionBegins
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

        public void OnSessionBegins(SessionBeginResult e)
        {
            (this.__Events[__evSessionBegins] as EventHandler<SessionBeginResult>)?.Invoke(this, e);

        }

        /// <summary>
        /// Will be called on incomming message
        /// </summary>
        public event EventHandler<MessageIncomeResult> Message
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

        public void OnMessage(MessageIncomeResult e)
        {
            (this.__Events[__evMessage] as EventHandler<MessageIncomeResult>)?.Invoke(this, e);

        }

        /// <summary>
        /// Will be called if a system call gets raised
        /// </summary>
        public event EventHandler<SystemCallEventArgs> SystemCall
        {
            add
            {
                this.__Events.AddHandler(__evSystemCall, value);
            }
            remove
            {
                this.__Events.RemoveHandler(__evSystemCall, value);
            }
        }

        public void OnSystemCall(SystemCallEventArgs e)
        {
            (this.__Events[__evSystemCall] as EventHandler<SystemCallEventArgs>)?.Invoke(this, e);

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
    }
}
