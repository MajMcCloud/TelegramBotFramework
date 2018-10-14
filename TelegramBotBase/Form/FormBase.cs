using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Form
{
    public class FormBase : IDisposable
    {
        public DeviceSession Device { get; set; }

        public MessageClient Client { get; set; }

        public bool CustomEventManagement { get; set; } = false;

        /// <summary>
        /// Gibt an, dass das Formular gewechselt wurde, es werden keine weiteren Events ausgeführt
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
        /// Wird nur aufgerufen, beim erstmaligen Laden des Formulares.
        /// </summary>
        public virtual async Task Init(params object[] args)
        {
            
        }

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

        public virtual async Task Load(MessageResult message)
        {

        }

        public virtual async Task Action(MessageResult message)
        {

        }


        public virtual async Task Render(MessageResult message)
        {

        }

        ///// <summary>
        ///// Navigiert zur neuen Form.
        ///// </summary>
        ///// <param name="newForm"></param>
        ///// <returns></returns>
        //public async Task NavigateTo(FormBase newForm)
        //{
        //    DeviceSession ds = this.Device;
        //    if (ds == null)
        //        return;

        //    this.FormSwitched = true;

        //    ds.ActiveForm = newForm;
        //    newForm.Client = this.Client;
        //    newForm.Device = ds;

        //    await this.Closed();

        //    await newForm.Opened();
        //}

        /// <summary>
        /// Navigiert zur neuen Form.
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

        public void Dispose()
        {
            this.Client = null;
            this.Device = null;
            this.FormSwitched = false;
        }
    }
}
