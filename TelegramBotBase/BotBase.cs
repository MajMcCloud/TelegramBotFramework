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
    public class BotBase<T>
        where T : FormBase
    {
        public MessageClient Client { get; set; }

        public String APIKey { get; set; } = "";


        public SessionBase Sessions { get; set; }

        /// <summary>
        /// Beinhaltet Systembefehle die immer erreichbar sind und nicht an die Formulare weitergereicht werden.  z.b. /start
        /// </summary>
        public List<String> SystemCalls { get; set; }

        private EventHandlerList __Events = new EventHandlerList();

        private static object __evSessionBegins = new object();

        private static object __evSystemCall = new object();

        private static object __evException = new object();

        private static object __evUnhandledCall = new object();

        public BotBase(String apiKey)
        {
            this.APIKey = apiKey;

            this.Client = new Base.MessageClient(this.APIKey);

            this.SystemCalls = new List<string>();

            this.Sessions = new SessionBase();
            this.Sessions.Client = this.Client;
        }

        public void Start()
        {
            if (this.Client == null)
                return;

            this.Client.Message += Client_Message;
            this.Client.Action += Client_Action;


            this.Client.TelegramClient.StartReceiving();
        }


        public void Stop()
        {
            if (this.Client == null)
                return;

            this.Client.TelegramClient.StopReceiving();
        }


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
            try
            {
                Client_TryMessage(sender, e);
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
        /// Wird aufgerufen wenn eine Session begonnen wird
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
