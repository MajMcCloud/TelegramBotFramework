using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotBase.Attributes;
using TelegramBotBase.Base;

namespace TelegramBotBase.Form
{
    [IgnoreState]
    public class PromptDialog : ModalDialog
    {
        public String Message { get; set; }

        public String Value { get; set; }

        private EventHandlerList __Events { get; set; } = new EventHandlerList();

        private static object __evCompleted { get; } = new object();

        public bool ShowBackButton { get; set; } = false;

        public String BackLabel { get; set; } = Localizations.Default.Language["PromptDialog_Back"];

        /// <summary>
        /// Contains the RAW received message.
        /// </summary>
        public Message ReceivedMessage { get; set; }

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

            if (!message.IsFirstHandler)
                return;

            if (this.ShowBackButton && message.MessageText == BackLabel)
            {
                await this.CloseForm();

                return;
            }

            if (this.Value == null)
            {
                this.Value = message.MessageText;

                ReceivedMessage = message.Message;
            }


        }

        public override async Task Render(MessageResult message)
        {

            if (this.Value == null)
            {
                if (this.ShowBackButton)
                {
                    ButtonForm bf = new ButtonForm();
                    bf.AddButtonRow(new ButtonBase(BackLabel, "back"));
                    await this.Device.Send(this.Message, (ReplyMarkupBase)bf);
                    return;
                }

                await this.Device.Send(this.Message);
                return;
            }


            message.Handled = true;

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
