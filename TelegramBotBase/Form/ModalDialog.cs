using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotBase.Form
{
    public class ModalDialog : FormBase
    {
        /// <summary>
        /// Contains the parent from where the modal dialog has been opened.
        /// </summary>
        public FormBase ParentForm { get; set; }

        /// <summary>
        /// This is a modal only function and does everything to close this form.
        /// </summary>
        public async Task CloseForm()
        {
            await this.CloseControls();

            await this.OnClosed(new EventArgs());


            await this.ParentForm?.ReturnFromModal(this);
        }
    }
}
