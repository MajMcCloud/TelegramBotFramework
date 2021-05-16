using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Attributes;
using TelegramBotBase.Base;

namespace TelegramBotBase.Form
{
    /// <summary>
    /// A prompt with a lot of buttons
    /// </summary>
    [IgnoreState]
    public class ArrayPromptDialog : FormBase
    {
        /// <summary>
        /// The message the users sees.
        /// </summary>
        public String Message { get; set; }

        /// <summary>
        /// An additional optional value.
        /// </summary>
        public object Tag { get; set; }

        public ButtonBase[][] Buttons { get; set; }

        [Obsolete]
        public Dictionary<String, FormBase> ButtonForms { get; set; } = new Dictionary<string, FormBase>();

        private EventHandlerList __Events { get; set; } = new EventHandlerList();

        private static object __evButtonClicked { get; } = new object();

        public ArrayPromptDialog()
        {

        }

        public ArrayPromptDialog(String Message)
        {
            this.Message = Message;
        }

        public ArrayPromptDialog(String Message, params ButtonBase[][] Buttons)
        {
            this.Message = Message;
            this.Buttons = Buttons;
        }

        public override async Task Action(MessageResult message)
        {
            var call = message.GetData<CallbackData>();

            message.Handled = true;

            if (!message.IsAction)
                return;

            await message.ConfirmAction();

            await message.DeleteMessage();

            var buttons = this.Buttons.Aggregate((a, b) => a.Union(b).ToArray()).ToList();

            if(call==null)
            {
                return;
            }

            ButtonBase button = buttons.FirstOrDefault(a => a.Value == call.Value);

            if (button == null)
            {
                return;
            }

            OnButtonClicked(new ButtonClickedEventArgs(button) { Tag = this.Tag });

            FormBase fb = ButtonForms.ContainsKey(call.Value) ? ButtonForms[call.Value] : null;

            if (fb != null)
            {
                await this.NavigateTo(fb);
            }
        }


        public override async Task Render(MessageResult message)
        {
            ButtonForm btn = new ButtonForm();

            foreach(var bl in this.Buttons)
            {
                btn.AddButtonRow(bl.Select(a => new ButtonBase(a.Text, CallbackData.Create("action", a.Value))).ToList());
            }

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
