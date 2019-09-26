using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotBase.Base;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace TelegramBotBase.Controls
{
    public class ButtonGrid : Base.ControlBase
    {

        public String Title { get; set; } = "Toggle";

        private bool RenderNecessary = true;

        private static readonly object __evButtonClicked = new object();

        private readonly EventHandlerList Events = new EventHandlerList();

        public ButtonForm ButtonsForm { get; set; }

        public int? MessageId { get; set; }

        /// <summary>
        /// Defines which type of Button Keyboard should be rendered.
        /// </summary>
        public eKeyboardType KeyboardType
        {
            get; private set;
        }

        public async Task SetKeyboardType(eKeyboardType type)
        {
            if (KeyboardType == type)
                return;

            this.RenderNecessary = true;

            Cleanup().Wait();

            KeyboardType = type;

        }

        private bool m_bVisible = true;

        public ButtonGrid()
        {
            this.ButtonsForm = new ButtonForm();


        }

        public ButtonGrid(ButtonForm form)
        {
            this.ButtonsForm = form;
        }


        public event EventHandler<ButtonClickedEventArgs> ButtonClicked
        {
            add
            {
                this.Events.AddHandler(__evButtonClicked, value);
            }
            remove
            {
                this.Events.RemoveHandler(__evButtonClicked, value);
            }
        }

        public void OnButtonClicked(ButtonClickedEventArgs e)
        {
            (this.Events[__evButtonClicked] as EventHandler<ButtonClickedEventArgs>)?.Invoke(this, e);
        }

        public async override Task Load(MessageResult result)
        {
            if (this.KeyboardType != eKeyboardType.ReplyKeyboard)
                return;


            var button = ButtonsForm.ToList().FirstOrDefault(a => a.Text == result.MessageText);

            if (button == null)
                return;

            OnButtonClicked(new ButtonClickedEventArgs(button));

            result.Handled = true;

        }

        public async override Task Action(MessageResult result, string value = null)
        {
            if (result.Handled)
                return;

            await result.ConfirmAction();

            //Find clicked button depending on Text or Value (depending on markup type)
            switch (this.KeyboardType)
            {
                case eKeyboardType.InlineKeyBoard:

                    var button = ButtonsForm.ToList().FirstOrDefault(a => a.Value == result.RawData);

                    if (button == null)
                        return;

                    OnButtonClicked(new ButtonClickedEventArgs(button));

                    result.Handled = true;

                    break;
            }

        }

        public async override Task Render(MessageResult result)
        {
            if (!this.RenderNecessary)
                return;

            Message m = null;
            if (this.MessageId != null)
            {
                switch (this.KeyboardType)
                {
                    //Reply Keyboard could only be updated with a new keyboard.
                    case eKeyboardType.ReplyKeyboard:
                        if (this.ButtonsForm.Count == 0)
                        {
                            await this.Device.Send("", new ReplyKeyboardRemove());
                            this.MessageId = null;
                        }
                        else
                        {
                            m = await this.Device.Send(this.Title, (ReplyKeyboardMarkup)this.ButtonsForm, disableNotification: true);
                        }

                        break;

                    case eKeyboardType.InlineKeyBoard:
                        m = await this.Device.Edit(this.MessageId.Value, this.Title, (InlineKeyboardMarkup)this.ButtonsForm);
                        break;
                }


            }
            else
            {
                switch (this.KeyboardType)
                {
                    case eKeyboardType.ReplyKeyboard:
                        m = await this.Device.Send(this.Title, (ReplyKeyboardMarkup)this.ButtonsForm, disableNotification: true);
                        break;

                    case eKeyboardType.InlineKeyBoard:
                        m = await this.Device.Send(this.Title, (InlineKeyboardMarkup)this.ButtonsForm, disableNotification: true);
                        break;
                }

                if (m != null)
                {
                    this.MessageId = m.MessageId;
                }
            }

            this.RenderNecessary = false;

        }


        public async override Task Cleanup()
        {
            if (this.MessageId == null)
                return;

            await this.Device.DeleteMessage(this.MessageId.Value);

            this.MessageId = null;

            if (this.KeyboardType == eKeyboardType.ReplyKeyboard)
            {
                await this.Device.Send("", new ReplyKeyboardRemove());
            }

        }

    }


}
