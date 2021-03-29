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
    [IgnoreState]
    public class ConfirmDialog : ModalDialog
    {
        /// <summary>
        /// The message the users sees.
        /// </summary>
        public String Message { get; set; }

        /// <summary>
        /// An additional optional value.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Automatically close form on button click
        /// </summary>
        public bool AutoCloseOnClick { get; set; } = true;

        public List<ButtonBase> Buttons { get; set; }

        private EventHandlerList __Events { get; set; } = new EventHandlerList();

        private static object __evButtonClicked { get; } = new object();

        public ConfirmDialog()
        {

        }

        public ConfirmDialog(String Message)
        {
            this.Message = Message;
            this.Buttons = new List<Form.ButtonBase>();
        }

        public ConfirmDialog(String Message, params ButtonBase[] Buttons)
        {
            this.Message = Message;
            this.Buttons = Buttons.ToList();
        }

        /// <summary>
        /// Adds one Button
        /// </summary>
        /// <param name="button"></param>
        public void AddButton(ButtonBase button)
        {
            this.Buttons.Add(button);
        }

        public override async Task Action(MessageResult message)
        {
            if (message.Handled)
                return;

            if (!message.IsFirstHandler)
                return;

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

            OnButtonClicked(new ButtonClickedEventArgs(button) { Tag = this.Tag });

            if (AutoCloseOnClick)
                await CloseForm();
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
