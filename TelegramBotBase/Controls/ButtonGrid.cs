using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Enums;
using TelegramBotBase.Exceptions;
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
        /// Optional. Requests clients to resize the keyboard vertically for optimal fit (e.g., make the keyboard smaller if there are just two rows of buttons). Defaults to false, in which case the custom keyboard is always of the same height as the app's standard keyboard.
        /// Source: https://core.telegram.org/bots/api#replykeyboardmarkup
        /// </summary>
        public bool ResizeKeyboard { get; set; } = false;

        public bool OneTimeKeyboard { get; set; } = false;

        public bool HideKeyboardOnCleanup { get; set; } = true;

        public bool DeletePreviousMessage { get; set; } = true;

        /// <summary>
        /// Defines which type of Button Keyboard should be rendered.
        /// </summary>
        public eKeyboardType KeyboardType
        {
            get
            {
                return m_eKeyboardType;
            }
            set
            {
                if (m_eKeyboardType != value)
                {
                    this.RenderNecessary = true;

                    Cleanup().Wait();

                    m_eKeyboardType = value;
                }

            }
        }

        private eKeyboardType m_eKeyboardType = eKeyboardType.ReplyKeyboard;

        private bool m_bVisible = true;

        public ButtonGrid()
        {
            this.ButtonsForm = new ButtonForm();


        }

        public ButtonGrid(eKeyboardType type) : this()
        {
            m_eKeyboardType = type;
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

            if (!result.IsFirstHandler)
                return;

            var button = ButtonsForm.ToList().FirstOrDefault(a => a.Text.Trim() == result.MessageText);

            if (button == null)
                return;

            OnButtonClicked(new ButtonClickedEventArgs(button));

            //Remove button click message
            if (this.DeletePreviousMessage)
                await Device.DeleteMessage(result.MessageId);

            result.Handled = true;

        }

        public async override Task Action(MessageResult result, string value = null)
        {
            if (result.Handled)
                return;

            if (!result.IsFirstHandler)
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


            switch (m_eKeyboardType)
            {
                case eKeyboardType.InlineKeyBoard:

                    if (ButtonsForm.Rows > 13)
                    {
                        throw new MaximumRowsReachedException() { Value = ButtonsForm.Rows, Maximum = 13 };
                    }

                    if (ButtonsForm.Rows > 8)
                    {
                        throw new MaximumColsException() { Value = ButtonsForm.Rows, Maximum = 8 };
                    }

                    break;
                case eKeyboardType.ReplyKeyboard:

                    if (ButtonsForm.Rows > 25)
                    {
                        throw new MaximumRowsReachedException() { Value = ButtonsForm.Rows, Maximum = 25 };
                    }

                    if (ButtonsForm.Rows > 12)
                    {
                        throw new MaximumColsException() { Value = ButtonsForm.Rows, Maximum = 12 };
                    }

                    break;
            }

            Message m = null;
            if (this.MessageId != null)
            {
                switch (this.KeyboardType)
                {
                    //Reply Keyboard could only be updated with a new keyboard.
                    case eKeyboardType.ReplyKeyboard:
                        if (this.ButtonsForm.Count == 0)
                        {
                            await this.Device.HideReplyKeyboard();
                            this.MessageId = null;
                        }
                        else
                        {
                            if (this.DeletePreviousMessage)
                                await this.Device.DeleteMessage(this.MessageId.Value);

                            var rkm = (ReplyKeyboardMarkup)this.ButtonsForm;
                            rkm.ResizeKeyboard = this.ResizeKeyboard;
                            rkm.OneTimeKeyboard = this.OneTimeKeyboard;
                            m = await this.Device.Send(this.Title, rkm, disableNotification: true);
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
                        var rkm = (ReplyKeyboardMarkup)this.ButtonsForm;
                        rkm.ResizeKeyboard = this.ResizeKeyboard;
                        rkm.OneTimeKeyboard = this.OneTimeKeyboard;
                        m = await this.Device.Send(this.Title, rkm, disableNotification: true);
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

        public override async Task Hidden(bool FormClose)
        {
            //Prepare for opening Modal, and comming back
            if (!FormClose)
            {
                this.Updated();
            }
        }

        /// <summary>
        /// Tells the control that it has been updated.
        /// </summary>
        public void Updated()
        {
            this.RenderNecessary = true;
        }

        public async override Task Cleanup()
        {
            if (this.MessageId == null)
                return;

            switch (this.KeyboardType)
            {
                case eKeyboardType.InlineKeyBoard:

                    await this.Device.DeleteMessage(this.MessageId.Value);

                    this.MessageId = null;

                    break;
                case eKeyboardType.ReplyKeyboard:

                    if (this.HideKeyboardOnCleanup)
                    {
                        await this.Device.HideReplyKeyboard();
                    }

                    this.MessageId = null;

                    break;
            }




        }

    }


}
