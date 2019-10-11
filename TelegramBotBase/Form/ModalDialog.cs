using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotBase.Form
{
    public class ModalDialog : FormBase
    {


        /// <summary>
        /// This is a modal only function and does everything to close this form.
        /// </summary>
        public async Task CloseForm()
        {

            await this.CloseControls();

            await this.OnClosed(new EventArgs());

        }
    }
}
