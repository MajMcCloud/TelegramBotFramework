using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Form.Navigation;
using TelegramBotBase.Sessions;
using static TelegramBotBase.Base.Async;

namespace TelegramBotBase.Form
{
    /// <summary>
    /// Base class for forms
    /// </summary>
    public class FormBase : IDisposable
    {

        public NavigationController NavigationController { get; set; }

        public DeviceSession Device { get; set; }

        public MessageClient Client { get; set; }

        /// <summary>
        /// has this formular already been disposed ?
        /// </summary>
        public bool IsDisposed { get; set; } = false;

        public List<ControlBase> Controls { get; set; }


        public EventHandlerList Events = new EventHandlerList();


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
            var handler = this.Events[__evInit]?.GetInvocationList().Cast<AsyncEventHandler<InitEventArgs>>();
            if (handler == null)
                return;

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
            var handler = this.Events[__evOpened]?.GetInvocationList().Cast<AsyncEventHandler<EventArgs>>();
            if (handler == null)
                return;

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
            var handler = this.Events[__evClosed]?.GetInvocationList().Cast<AsyncEventHandler<EventArgs>>();
            if (handler == null)
                return;

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
        /// Get invoked when a modal child from has been closed.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual async Task ReturnFromModal(ModalDialog modal)
        {

        }


        /// <summary>
        /// Pre to form close, cleanup all controls
        /// </summary>
        /// <returns></returns>
        public async Task CloseControls()
        {
            foreach (var b in this.Controls)
            {
                b.Cleanup().Wait();
            }
        }

        public virtual async Task PreLoad(MessageResult message)
        {

        }

        /// <summary>
        /// Gets invoked if a message was sent or an action triggered
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual async Task LoadControls(MessageResult message)
        {
            //Looking for the control by id, if not listened, raise event for all
            if (message.RawData?.StartsWith("#c") ?? false)
            {
                var c = this.Controls.FirstOrDefault(a => a.ControlID == message.RawData.Split('_')[0]);
                if (c != null)
                {
                    await c.Load(message);
                    return;
                }
            }

            foreach (var b in this.Controls)
            {
                if (!b.Enabled)
                    continue;

                await b.Load(message);
            }
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
        /// Gets invoked, when a messages has been edited.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual async Task Edited(MessageResult message)
        {

        }


        /// <summary>
        /// Gets invoked if the user clicked a button.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual async Task ActionControls(MessageResult message)
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

                if (message.Handled)
                    return;
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
        public virtual async Task RenderControls(MessageResult message)
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
        public virtual async Task NavigateTo(FormBase newForm, params object[] args)
        {
            DeviceSession ds = this.Device;
            if (ds == null)
                return;

            ds.FormSwitched = true;

            ds.PreviousForm = ds.ActiveForm;

            ds.ActiveForm = newForm;
            newForm.Client = this.Client;
            newForm.Device = ds;

            //Notify prior to close
            foreach (var b in this.Controls)
            {
                if (!b.Enabled)
                    continue;

                await b.Hidden(true);
            }

            this.CloseControls().Wait();

            await this.OnClosed(new EventArgs());

            await newForm.OnInit(new InitEventArgs(args));

            await newForm.OnOpened(new EventArgs());
        }

        /// <summary>
        /// Opens this form modal, but don't closes the original ones
        /// </summary>
        /// <param name="newForm"></param>
        /// <returns></returns>
        public virtual async Task OpenModal(ModalDialog newForm, params object[] args)
        {
            DeviceSession ds = this.Device;
            if (ds == null)
                return;

            var parentForm = this;

            ds.FormSwitched = true;

            ds.PreviousForm = ds.ActiveForm;

            ds.ActiveForm = newForm;
            newForm.Client = parentForm.Client;
            newForm.Device = ds;
            newForm.ParentForm = parentForm;

            newForm.Closed += async (s, en) =>
            {
                await CloseModal(newForm, parentForm);
            };

            foreach (var b in this.Controls)
            {
                if (!b.Enabled)
                    continue;

                await b.Hidden(false);
            }

            await newForm.OnInit(new InitEventArgs(args));

            await newForm.OnOpened(new EventArgs());
        }

        public async Task CloseModal(ModalDialog modalForm, FormBase oldForm)
        {
            DeviceSession ds = this.Device;
            if (ds == null)
                return;

            if (modalForm == null)
                throw new Exception("No modal form");

            ds.FormSwitched = true;

            ds.PreviousForm = ds.ActiveForm;

            ds.ActiveForm = oldForm;
        }

        /// <summary>
        /// Adds a control to the formular and sets its ID and Device.
        /// </summary>
        /// <param name="control"></param>
        public void AddControl(ControlBase control)
        {
            //Duplicate check
            if (this.Controls.Contains(control))
                throw new ArgumentException("Control has been already added.");

            control.ID = this.Controls.Count + 1;
            control.Device = this.Device;
            this.Controls.Add(control);

            control.Init();
        }

        /// <summary>
        /// Removes control from the formular and runs a cleanup on it.
        /// </summary>
        /// <param name="control"></param>
        public void RemoveControl(ControlBase control)
        {
            if (!this.Controls.Contains(control))
                return;

            control.Cleanup().Wait();

            this.Controls.Remove(control);
        }

        /// <summary>
        /// Removes all controls.
        /// </summary>
        public void RemoveAllControls()
        {
            foreach(var c in this.Controls)
            {
                c.Cleanup().Wait();

                this.Controls.Remove(c);
            }
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
