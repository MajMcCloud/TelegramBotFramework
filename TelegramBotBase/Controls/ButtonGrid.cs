using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
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

        public String Title { get; set; } = Localizations.Default.Language["ButtonGrid_Title"];

        public String ConfirmationText { get; set; } = "";

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

        public ParseMode MessageParseMode { get; set; } = ParseMode.Default;

        /// <summary>
        /// Enables automatic paging of buttons when the amount of rows is exceeding the limits.
        /// </summary>
        public bool EnablePaging { get; set; } = false;

        /// <summary>
        /// Aktueller Seitenindex
        /// </summary>
        public int CurrentPageIndex { get; set; } = 0;

        public String PreviousPageLabel = Localizations.Default.Language["ButtonGrid_PreviousPage"];

        public String NextPageLabel = Localizations.Default.Language["ButtonGrid_NextPage"];

        public String NoItemsLabel = Localizations.Default.Language["ButtonGrid_NoItems"];

        public List<ButtonBase> HeadLayoutButtonRow { get; set; }

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

            var button = HeadLayoutButtonRow?.FirstOrDefault(a => a.Text.Trim() == result.MessageText) ?? ButtonsForm.ToList().FirstOrDefault(a => a.Text.Trim() == result.MessageText);

            if (button == null)
            {
                if (result.MessageText != null)
                {
                    if (result.MessageText == PreviousPageLabel)
                    {
                        if (this.CurrentPageIndex > 0)
                            this.CurrentPageIndex--;

                        this.Updated();
                    }
                    else if (result.MessageText == NextPageLabel)
                    {
                        if (this.CurrentPageIndex < this.PageCount - 1)
                            this.CurrentPageIndex++;

                        this.Updated();
                    }
                }

                return;
            }


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

            await result.ConfirmAction(this.ConfirmationText ?? "");

            //Find clicked button depending on Text or Value (depending on markup type)
            switch (this.KeyboardType)
            {
                case eKeyboardType.InlineKeyBoard:

                    var button = HeadLayoutButtonRow?.FirstOrDefault(a => a.Value == result.RawData) ?? ButtonsForm.ToList().FirstOrDefault(a => a.Value == result.RawData);

                    if (button == null)
                    {
                        switch (result.RawData)
                        {
                            case "$previous$":

                                if (this.CurrentPageIndex > 0)
                                    this.CurrentPageIndex--;

                                this.Updated();

                                break;
                            case "$next$":

                                if (this.CurrentPageIndex < this.PageCount - 1)
                                    this.CurrentPageIndex++;

                                this.Updated();

                                break;
                        }


                        return;
                    }

                    OnButtonClicked(new ButtonClickedEventArgs(button));

                    result.Handled = true;

                    break;
            }

        }

        /// <summary>
        /// This method checks of the amount of buttons
        /// </summary>
        private void CheckGrid()
        {
            switch (m_eKeyboardType)
            {
                case eKeyboardType.InlineKeyBoard:

                    if (ButtonsForm.Rows > Constants.Telegram.MaxInlineKeyBoardRows && !this.EnablePaging)
                    {
                        throw new MaximumRowsReachedException() { Value = ButtonsForm.Rows, Maximum = Constants.Telegram.MaxInlineKeyBoardRows };
                    }

                    if (ButtonsForm.Cols > Constants.Telegram.MaxInlineKeyBoardCols)
                    {
                        throw new MaximumColsException() { Value = ButtonsForm.Rows, Maximum = Constants.Telegram.MaxInlineKeyBoardCols };
                    }

                    break;

                case eKeyboardType.ReplyKeyboard:

                    if (ButtonsForm.Rows > Constants.Telegram.MaxReplyKeyboardRows && !this.EnablePaging)
                    {
                        throw new MaximumRowsReachedException() { Value = ButtonsForm.Rows, Maximum = Constants.Telegram.MaxReplyKeyboardRows };
                    }

                    if (ButtonsForm.Cols > Constants.Telegram.MaxReplyKeyboardCols)
                    {
                        throw new MaximumColsException() { Value = ButtonsForm.Rows, Maximum = Constants.Telegram.MaxReplyKeyboardCols };
                    }

                    break;
            }
        }

        public async override Task Render(MessageResult result)
        {
            if (!this.RenderNecessary)
                return;

            //Check for rows and column limits
            CheckGrid();

            this.RenderNecessary = false;

            Message m = null;

            ButtonForm form = this.ButtonsForm.Duplicate();


            if (this.EnablePaging)
            {
                form = GeneratePagingView(form);
            }

            if (this.HeadLayoutButtonRow != null && HeadLayoutButtonRow.Count > 0)
            {
                form.InsertButtonRow(0, this.HeadLayoutButtonRow);
            }

            switch (this.KeyboardType)
            {
                //Reply Keyboard could only be updated with a new keyboard.
                case eKeyboardType.ReplyKeyboard:

                    if (this.MessageId != null)
                    {
                        if (form.Count == 0)
                        {
                            await this.Device.HideReplyKeyboard();
                            this.MessageId = null;
                            return;
                        }

                        if (this.DeletePreviousMessage)
                            await this.Device.DeleteMessage(this.MessageId.Value);
                    }

                    if (form.Count == 0)
                        return;


                    var rkm = (ReplyKeyboardMarkup)form;
                    rkm.ResizeKeyboard = this.ResizeKeyboard;
                    rkm.OneTimeKeyboard = this.OneTimeKeyboard;
                    m = await this.Device.Send(this.Title, rkm, disableNotification: true, parseMode: MessageParseMode, MarkdownV2AutoEscape: false);

                    break;

                case eKeyboardType.InlineKeyBoard:

                    if (this.MessageId != null)
                    {
                        m = await this.Device.Edit(this.MessageId.Value, this.Title, (InlineKeyboardMarkup)form);
                    }
                    else
                    {
                        m = await this.Device.Send(this.Title, (InlineKeyboardMarkup)form, disableNotification: true, parseMode: MessageParseMode, MarkdownV2AutoEscape: false);
                    }

                    break;
            }

            if (m != null)
            {
                this.MessageId = m.MessageId;
            }


        }

        private ButtonForm GeneratePagingView(ButtonForm dataForm)
        {

            ButtonForm bf = new ButtonForm();


            for (int i = 0; i < this.MaximumRow - LayoutRows; i++)
            {
                int it = (this.CurrentPageIndex * (this.MaximumRow - LayoutRows)) + i;

                if (it > dataForm.Rows - 1)
                    break;

                bf.AddButtonRow(dataForm[it]);
            }

            //No Items 
            if (this.ButtonsForm.Count == 0)
            {
                bf.AddButtonRow(new ButtonBase(NoItemsLabel, "$"));
            }
            
            
            bf.InsertButtonRow(0, new ButtonBase(PreviousPageLabel, "$previous$"), new ButtonBase(String.Format(Localizations.Default.Language["ButtonGrid_CurrentPage"], this.CurrentPageIndex + 1, this.PageCount), "$site$"), new ButtonBase(NextPageLabel, "$next$"));

            bf.AddButtonRow(new ButtonBase(PreviousPageLabel, "$previous$"), new ButtonBase(String.Format(Localizations.Default.Language["ButtonGrid_CurrentPage"], this.CurrentPageIndex + 1, this.PageCount), "$site$"), new ButtonBase(NextPageLabel, "$next$"));

            return bf;
        }

        public bool PagingNecessary
        {
            get
            {
                if ((this.KeyboardType == eKeyboardType.InlineKeyBoard && ButtonsForm.Rows > Constants.Telegram.MaxInlineKeyBoardRows) | (this.KeyboardType == eKeyboardType.ReplyKeyboard && ButtonsForm.Rows > Constants.Telegram.MaxReplyKeyboardRows))
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Returns the maximum number of rows
        /// </summary>
        public int MaximumRow
        {
            get
            {
                switch (this.KeyboardType)
                {
                    case eKeyboardType.InlineKeyBoard:
                        return Constants.Telegram.MaxInlineKeyBoardRows;

                    case eKeyboardType.ReplyKeyboard:
                        return Constants.Telegram.MaxReplyKeyboardRows;

                    default:
                        return 0;
                }
            }
        }

        /// <summary>
        /// Contains the Number of Rows which are used by the layout.
        /// </summary>
        private int LayoutRows
        {
            get
            {
                int layoutRows = 2;

                if (this.HeadLayoutButtonRow != null && this.HeadLayoutButtonRow.Count > 0)
                    layoutRows++;

                return layoutRows;
            }
        }

        public int PageCount
        {
            get
            {
                if (this.ButtonsForm.Count == 0)
                    return 1;

                return (int)Math.Ceiling((decimal)(this.ButtonsForm.Rows / (decimal)(MaximumRow - 3)));
            }
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
