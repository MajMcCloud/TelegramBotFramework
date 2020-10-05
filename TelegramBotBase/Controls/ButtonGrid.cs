using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.SymbolStore;
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
using static TelegramBotBase.Base.Async;

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

        /// <summary>
        /// Parsemode of the message.
        /// </summary>
        public ParseMode MessageParseMode { get; set; } = ParseMode.Default;

        /// <summary>
        /// Enables automatic paging of buttons when the amount of rows is exceeding the limits.
        /// </summary>
        public bool EnablePaging { get; set; } = false;


        /// <summary>
        /// Enabled a search function.
        /// </summary>
        public bool EnableSearch { get; set; } = false;

        public String SearchQuery { get; set; }

        public eNavigationBarVisibility NavigationBarVisibility { get; set; } = eNavigationBarVisibility.always;


        /// <summary>
        /// Index of the current page
        /// </summary>
        public int CurrentPageIndex { get; set; } = 0;

        public String PreviousPageLabel = Localizations.Default.Language["ButtonGrid_PreviousPage"];

        public String NextPageLabel = Localizations.Default.Language["ButtonGrid_NextPage"];

        public String NoItemsLabel = Localizations.Default.Language["ButtonGrid_NoItems"];

        public String SearchLabel = Localizations.Default.Language["ButtonGrid_SearchFeature"];

        /// <summary>
        /// Layout of the buttons which should be displayed always on top.
        /// </summary>
        public List<ButtonBase> HeadLayoutButtonRow { get; set; }

        /// <summary>
        /// Layout of columns which should be displayed below the header
        /// </summary>
        public List<ButtonBase> SubHeadLayoutButtonRow { get; set; }

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


        public event AsyncEventHandler<ButtonClickedEventArgs> ButtonClicked
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

        public async Task OnButtonClicked(ButtonClickedEventArgs e)
        {
            var handler = this.Events[__evButtonClicked].GetInvocationList().Cast<AsyncEventHandler<ButtonClickedEventArgs>>();
            foreach (var h in handler)
            {
                await Async.InvokeAllAsync<ButtonClickedEventArgs>(h, this, e);
            }
        }

        public async override Task Load(MessageResult result)
        {
            if (this.KeyboardType != eKeyboardType.ReplyKeyboard)
                return;

            if (!result.IsFirstHandler)
                return;

            var button = HeadLayoutButtonRow?.FirstOrDefault(a => a.Text.Trim() == result.MessageText)
                        ?? SubHeadLayoutButtonRow?.FirstOrDefault(a => a.Text.Trim() == result.MessageText)
                        ?? ButtonsForm.ToList().FirstOrDefault(a => a.Text.Trim() == result.MessageText);

            if (button == null)
            {
                if (result.MessageText == null)
                    return;

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
                else if (this.EnableSearch)
                {
                    if (result.MessageText.StartsWith("🔍"))
                    {
                        //Sent note about searching
                        if (this.SearchQuery == null)
                        {
                            await this.Device.Send(this.SearchLabel);
                        }

                        this.SearchQuery = null;
                        this.Updated();
                        return;
                    }

                    this.SearchQuery = result.MessageText;

                    if (this.SearchQuery != null && this.SearchQuery != "")
                    {
                        this.CurrentPageIndex = 0;
                        this.Updated();
                    }

                }



                return;
            }


            await OnButtonClicked(new ButtonClickedEventArgs(button));

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

                    var button = HeadLayoutButtonRow?.FirstOrDefault(a => a.Value == result.RawData)
                                ?? SubHeadLayoutButtonRow?.FirstOrDefault(a => a.Value == result.RawData)
                                ?? ButtonsForm.ToList().FirstOrDefault(a => a.Value == result.RawData);

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

                    await OnButtonClicked(new ButtonClickedEventArgs(button));

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

            ButtonForm form = this.ButtonsForm;

            if (this.EnableSearch && this.SearchQuery != null && this.SearchQuery != "")
            {
                form = form.FilterDuplicate(this.SearchQuery, true);
            }
            else
            {
                form = form.Duplicate();
            }

            if (this.EnablePaging)
            {
                form = GeneratePagingView(form);
            }

            if (this.HeadLayoutButtonRow != null && HeadLayoutButtonRow.Count > 0)
            {
                form.InsertButtonRow(0, this.HeadLayoutButtonRow);
            }

            if (this.SubHeadLayoutButtonRow != null && SubHeadLayoutButtonRow.Count > 0)
            {
                if (this.IsNavigationBarVisible)
                {
                    form.InsertButtonRow(2, this.SubHeadLayoutButtonRow);
                }
                else
                {
                    form.InsertButtonRow(1, this.SubHeadLayoutButtonRow);
                }
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

            if (this.IsNavigationBarVisible)
            {
                //🔍
                List<ButtonBase> lst = new List<ButtonBase>();
                lst.Add(new ButtonBase(PreviousPageLabel, "$previous$"));
                lst.Add(new ButtonBase(String.Format(Localizations.Default.Language["ButtonGrid_CurrentPage"], this.CurrentPageIndex + 1, this.PageCount), "$site$"));
                lst.Add(new ButtonBase(NextPageLabel, "$next$"));

                if (this.EnableSearch)
                {
                    lst.Insert(2, new ButtonBase("🔍 " + (this.SearchQuery ?? ""), "$search$"));
                }

                bf.InsertButtonRow(0, lst);

                bf.AddButtonRow(lst);
            }

            return bf;
        }

        public bool PagingNecessary
        {
            get
            {
                if (this.KeyboardType == eKeyboardType.InlineKeyBoard && TotalRows > Constants.Telegram.MaxInlineKeyBoardRows)
                {
                    return true;
                }

                if (this.KeyboardType == eKeyboardType.ReplyKeyboard && TotalRows > Constants.Telegram.MaxReplyKeyboardRows)
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsNavigationBarVisible
        {
            get
            {
                if (this.NavigationBarVisibility == eNavigationBarVisibility.always | (this.NavigationBarVisibility == eNavigationBarVisibility.auto && PagingNecessary))
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
        /// Returns the number of all rows (layout + navigation + content);
        /// </summary>
        public int TotalRows
        {
            get
            {
                return this.LayoutRows + ButtonsForm.Rows;
            }
        }


        /// <summary>
        /// Contains the Number of Rows which are used by the layout.
        /// </summary>
        private int LayoutRows
        {
            get
            {
                int layoutRows = 0;

                if (this.NavigationBarVisibility == eNavigationBarVisibility.always | this.NavigationBarVisibility == eNavigationBarVisibility.auto)
                    layoutRows += 2;

                if (this.HeadLayoutButtonRow != null && this.HeadLayoutButtonRow.Count > 0)
                    layoutRows++;

                if (this.SubHeadLayoutButtonRow != null && this.SubHeadLayoutButtonRow.Count > 0)
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

                var bf = this.ButtonsForm;

                if (this.EnableSearch && this.SearchQuery != null && this.SearchQuery != "")
                {
                    bf = bf.FilterDuplicate(this.SearchQuery);
                }

                if (bf.Rows == 0)
                    return 1;

                return (int)Math.Ceiling((decimal)(bf.Rows / (decimal)(MaximumRow - 3)));
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
