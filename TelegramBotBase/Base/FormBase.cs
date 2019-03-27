using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Form
{
    /// <summary>
    /// Base class for forms
    /// </summary>
    public class FormBase : IDisposable
    {
        public DeviceSession Device { get; set; }

        public MessageClient Client { get; set; }

        public bool CustomEventManagement { get; set; } = false;

        /// <summary>
        /// contains if the form has been switched (navigated)
        /// </summary>
        public bool FormSwitched { get; set; } = false;

        public List<ControlBase> Controls { get; set; }

        public FormBase()
        {
            this.Controls = new List<Base.ControlBase>();
        }

        public FormBase(MessageClient Client): this()
        {
            this.Client = Client;
        }

        /// <summary>
        /// Will get called at the initialization (once per context)
        /// </summary>
        public virtual async Task Init(params object[] args)
        {
            
        }

        /// <summary>
        /// Gets invoked if gets navigated to this form
        /// </summary>
        /// <returns></returns>
        public virtual async Task Opened()
        {

        }

        public virtual async Task Closed()
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

            this.FormSwitched = true;

            ds.ActiveForm = newForm;
            newForm.Client = this.Client;
            newForm.Device = ds;

            await newForm.Init(args);

            await this.Closed();

            await newForm.Opened();
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            this.Client = null;
            this.Device = null;
            this.FormSwitched = false;
        }
    }
}
