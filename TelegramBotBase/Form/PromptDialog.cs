using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;

namespace TelegramBotBase.Form
{
    public class PromptDialog : ModalDialog
    {
        public String Message { get; set; }

        public String Value { get; set; }

        private EventHandlerList __Events { get; set; } = new EventHandlerList();

        private static object __evCompleted { get; } = new object();


        public PromptDialog()
        {

        }

        public PromptDialog(String Message)
        {
            this.Message = Message;
        }

        public async override Task Load(MessageResult message)
        {
            if (message.Handled)
                return;

            if (this.Value == null)
            {
                this.Value = message.MessageText;
            }


        }

        public override async Task Render(MessageResult message)
        {

            if (this.Value == null)
            {
                await this.Device.Send(this.Message);
                return;
            }


            OnCompleted(new EventArgs());

            await this.CloseForm();
        }


        public event EventHandler<EventArgs> Completed
        {
            add
            {
                this.__Events.AddHandler(__evCompleted, value);
            }
            remove
            {
                this.__Events.RemoveHandler(__evCompleted, value);
            }
        }

        public void OnCompleted(EventArgs e)
        {
            (this.__Events[__evCompleted] as EventHandler<EventArgs>)?.Invoke(this, e);
        }

    }
}
