using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;

namespace TelegramBotBase.Form
{
    public class PromptDialog : FormBase
    {
        public String Message { get; set; }


        public List<ButtonBase> Buttons { get; set; }

        public Dictionary<String, FormBase> ButtonForms { get; set; } = new Dictionary<string, FormBase>();

        private EventHandlerList __Events { get; set; } = new EventHandlerList();

        private static object __evButtonClicked { get; } = new object();

        public PromptDialog()
        {

        }

        public PromptDialog(String Message)
        {
            this.Message = Message;
            this.Buttons = new List<Form.ButtonBase>();
        }

        public PromptDialog(String Message, params ButtonBase[] Buttons)
        {
            this.Message = Message;
            this.Buttons = Buttons.ToList();
        }

        public override async Task Action(MessageResult message)
        {
            var call = message.GetData<CallbackData>();
            if (call == null)
                return;

            message.Handled = true;

            await message.ConfirmAction();

            await message.DeleteMessage();

            ButtonBase button = this.Buttons.FirstOrDefault(a => a.Value == call.Value);

            if (button == null)
            {
                return;
            }

            OnButtonClicked(new ButtonClickedEventArgs(button));

            FormBase fb = ButtonForms.ContainsKey(call.Value) ? ButtonForms[call.Value] : null;

            if (fb != null)
            {
                await this.NavigateTo(fb);
            }
        }


        public override async Task Render(MessageResult message)
        {
            ButtonForm btn = new ButtonForm();

            var buttons = this.Buttons.Select(a => new ButtonBase(a.Text, CallbackData.Create("action", a.Value))).ToList();
            btn.AddButtonRow(buttons);

            await this.Device.Send(this.Message, btn);
        }


        public event EventHandler<ButtonClickedEventArgs> ButtonClicked
        {
            add
            {
                this.__Events.AddHandler(__evButtonClicked, value);
            }
            remove
            {
                this.__Events.RemoveHandler(__evButtonClicked, value);
            }
        }

        public void OnButtonClicked(ButtonClickedEventArgs e)
        {
            (this.__Events[__evButtonClicked] as EventHandler<ButtonClickedEventArgs>)?.Invoke(this, e);
        }

    }
}
