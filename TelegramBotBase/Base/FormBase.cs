using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Sessions;
using static TelegramBotBase.Base.Async;

namespace TelegramBotBase.Form
{
    /// <summary>
    /// Base class for forms
    /// </summary>
    public class FormBase : IDisposable
    {

        public DeviceSession Device { get; set; }

        public MessageClient Client { get; set; }

        /// <summary>
        /// has this formular already been disposed ?
        /// </summary>
        public bool IsDisposed { get; set; } = false;

        public List<ControlBase> Controls { get; set; }


        private EventHandlerList Events = new EventHandlerList();


        private static object __evInit = new object();

        private static object __evOpened = new object();

        private static object __evClosed = new object();


        public FormBase()
        {
            this.Controls = new List<Base.ControlBase>();
        }

        public FormBase(MessageClient Client) : this()
        {
            this.Client = Client;
        }


        public async Task OnInit(InitEventArgs e)
        {
            if (this.Events[__evInit] == null)
                return;

            var handler = this.Events[__evInit].GetInvocationList().Cast<AsyncEventHandler<InitEventArgs>>();
            foreach (var h in handler)
            {
                await Async.InvokeAllAsync<InitEventArgs>(h, this, e);
            }
        }

        ///// <summary>
        ///// Will get called at the initialization (once per context)
        ///// </summary>
        public event AsyncEventHandler<InitEventArgs> Init
        {
            add
            {
                this.Events.AddHandler(__evInit, value);
            }
            remove
            {
                this.Events.RemoveHandler(__evInit, value);
            }
        }



        public async Task OnOpened(EventArgs e)
        {
            if (this.Events[__evOpened] == null)
                return;

            var handler = this.Events[__evOpened].GetInvocationList().Cast<AsyncEventHandler<EventArgs>>();
            foreach (var h in handler)
            {
                await Async.InvokeAllAsync<EventArgs>(h, this, e);
            }
        }

        /// <summary>
        /// Gets invoked if gets navigated to this form
        /// </summary>
        /// <returns></returns>
        public event AsyncEventHandler<EventArgs> Opened
        {
            add
            {
                this.Events.AddHandler(__evOpened, value);
            }
            remove
            {
                this.Events.RemoveHandler(__evOpened, value);
            }
        }

        public async Task OnClosed(EventArgs e)
        {
            if (this.Events[__evClosed] == null)
                return;

            var handler = this.Events[__evClosed].GetInvocationList().Cast<AsyncEventHandler<EventArgs>>();
            foreach (var h in handler)
            {
                await Async.InvokeAllAsync<EventArgs>(h, this, e);
            }
        }

        /// <summary>
        /// Form has been closed (left)
        /// </summary>
        /// <returns></returns>
        public event AsyncEventHandler<EventArgs> Closed
        {
            add
            {
                this.Events.AddHandler(__evClosed, value);
            }
            remove
            {
                this.Events.RemoveHandler(__evClosed, value);
            }
        }


        /// <summary>
        /// Pre to form close, cleanup all controls
        /// </summary>
        /// <returns></returns>
        public async Task CloseControls()
        {
            foreach (var b in this.Controls)
            {
                await b.Cleanup();
            }
        }

        public virtual async Task PreLoad(MessageResult message)
        {

        }

        /// <summary>
        /// Gets invoked if the form gets loaded and on every message belongs to this context
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual async Task Load(MessageResult message)
        {

        }

        /// <summary>
        /// Gets invoked if the user clicked a button.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task ActionControls(MessageResult message)
        {
            //Looking for the control by id, if not listened, raise event for all
            if (message.RawData.StartsWith("#c"))
            {
                var c = this.Controls.FirstOrDefault(a => a.ControlID == message.RawData.Split('_')[0]);
                if (c != null)
                {
                    await c.Action(message, message.RawData.Split('_')[1]);
                    return;
                }
            }

            foreach (var b in this.Controls)
            {
                if (!b.Enabled)
                    continue;

                await b.Action(message);
            }
        }

        /// <summary>
        /// Gets invoked if the user has clicked a button.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual async Task Action(MessageResult message)
        {

        }

        /// <summary>
        /// Gets invoked if the user has sent some media (Photo, Audio, Video, Contact, Location, Document)
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual async Task SentData(DataResult message)
        {

        }

        /// <summary>
        /// Gets invoked at the end of the cycle to "Render" text, images, buttons, etc...
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task RenderControls(MessageResult message)
        {
            foreach (var b in this.Controls)
            {
                if (!b.Enabled)
                    continue;

                await b.Render(message);
            }
        }

        /// <summary>
        /// Gets invoked at the end of the cycle to "Render" text, images, buttons, etc...
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual async Task Render(MessageResult message)
        {

        }



        /// <summary>
        /// Navigates to a new form
        /// </summary>
        /// <param name="newForm"></param>
        /// <returns></returns>
        public async Task NavigateTo(FormBase newForm, params object[] args)
        {
            DeviceSession ds = this.Device;
            if (ds == null)
                return;

            ds.FormSwitched = true;

            ds.PreviousForm = ds.ActiveForm;

            ds.ActiveForm = newForm;
            newForm.Client = this.Client;
            newForm.Device = ds;

            await newForm.OnInit(new InitEventArgs(args));

            await this.CloseControls();

            await this.OnClosed(new EventArgs());

            await newForm.OnOpened(new EventArgs());
        }

        public void AddControl(ControlBase control)
        {
            control.ID = this.Controls.Count + 1;
            control.Device = this.Device;
            this.Controls.Add(control);
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            this.Client = null;
            this.Device = null;
            this.IsDisposed = true;
        }
    }
}
