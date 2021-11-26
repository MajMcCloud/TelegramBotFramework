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
using TelegramBotBase.Datasources;
using TelegramBotBase.Enums;
using TelegramBotBase.Exceptions;
using TelegramBotBase.Form;
using static TelegramBotBase.Base.Async;

namespace TelegramBotBase.Controls.Hybrid
{
    public class CheckedButtonList : Base.ControlBase
    {

        public String Title { get; set; } = Localizations.Default.Language["ButtonGrid_Title"];

        public String ConfirmationText { get; set; } = "";

        private bool RenderNecessary = true;

        private static readonly object __evButtonClicked = new object();

        private static readonly object __evCheckedChanged = new object();

        private readonly EventHandlerList Events = new EventHandlerList();

        [Obsolete("This property is obsolete. Please use the DataSource property instead.")]
        public ButtonForm ButtonsForm
        {
            get
            {
                return DataSource.ButtonForm;
            }
            set
            {
                DataSource = new ButtonFormDataSource(value);
            }
        }

        /// <summary>
        /// Data source of the items.
        /// </summary>
        public ButtonFormDataSource DataSource { get; set; }

        List<int> CheckedRows { get; set; } = new List<int>();

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
        /// Removes the reply message from a user.
        /// </summary>
        public bool DeleteReplyMessage { get; set; } = true;

        /// <summary>
        /// Parsemode of the message.
        /// </summary>
        public ParseMode MessageParseMode { get; set; } = ParseMode.Markdown;

        /// <summary>
        /// Enables automatic paging of buttons when the amount of rows is exceeding the limits.
        /// </summary>
        public bool EnablePaging { get; set; } = false;


        ///// <summary>
        ///// Enabled a search function.
        ///// </summary>
        //public bool EnableSearch { get; set; } = false;

        //public String SearchQuery { get; set; }

        public eNavigationBarVisibility NavigationBarVisibility { get; set; } = eNavigationBarVisibility.always;


        /// <summary>
        /// Index of the current page
        /// </summary>
        public int CurrentPageIndex { get; set; } = 0;

        public String PreviousPageLabel = Localizations.Default.Language["ButtonGrid_PreviousPage"];

        public String NextPageLabel = Localizations.Default.Language["ButtonGrid_NextPage"];

        public String NoItemsLabel = Localizations.Default.Language["ButtonGrid_NoItems"];

        //public String SearchLabel = Localizations.Default.Language["ButtonGrid_SearchFeature"];

        public String CheckedIconLabel { get; set; } = "✅";

        public String UncheckedIconLabel { get; set; } = "◻️";

        /// <summary>
        /// Layout of the buttons which should be displayed always on top.
        /// </summary>
        public ButtonRow HeadLayoutButtonRow { get; set; }

        /// <summary>
        /// Layout of columns which should be displayed below the header
        /// </summary>
        public ButtonRow SubHeadLayoutButtonRow { get; set; }

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

        public CheckedButtonList()
        {
            this.DataSource = new ButtonFormDataSource();


        }

        public CheckedButtonList(eKeyboardType type) : this()
        {
            m_eKeyboardType = type;
        }


        public CheckedButtonList(ButtonForm form)
        {
            this.DataSource = new ButtonFormDataSource(form);
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
            var handler = this.Events[__evButtonClicked]?.GetInvocationList().Cast<AsyncEventHandler<ButtonClickedEventArgs>>();
            if (handler == null)
                return;

            foreach (var h in handler)
            {
                await Async.InvokeAllAsync<ButtonClickedEventArgs>(h, this, e);
            }
        }

        public event AsyncEventHandler<CheckedChangedEventArgs> CheckedChanged
        {
            add
            {
                this.Events.AddHandler(__evCheckedChanged, value);
            }
            remove
            {
                this.Events.RemoveHandler(__evCheckedChanged, value);
            }
        }

        public async Task OnCheckedChanged(CheckedChangedEventArgs e)
        {
            var handler = this.Events[__evCheckedChanged]?.GetInvocationList().Cast<AsyncEventHandler<CheckedChangedEventArgs>>();
            if (handler == null)
                return;

            foreach (var h in handler)
            {
                await Async.InvokeAllAsync<CheckedChangedEventArgs>(h, this, e);
            }
        }

        public async override Task Load(MessageResult result)
        {
            if (this.KeyboardType != eKeyboardType.ReplyKeyboard)
                return;

            if (!result.IsFirstHandler)
                return;

            if (result.MessageText == null || result.MessageText == "")
                return;

            var matches = new List<ButtonRow>();
            ButtonRow match = null;
            int index = -1;

            if (HeadLayoutButtonRow?.Matches(result.MessageText) ?? false)
            {
                match = HeadLayoutButtonRow;
                goto check;
            }

            if (SubHeadLayoutButtonRow?.Matches(result.MessageText) ?? false)
            {
                match = SubHeadLayoutButtonRow;
                goto check;
            }

            var br = DataSource.FindRow(result.MessageText);
            if (br != null)
            {
                match = br.Item1;
                index = br.Item2;
            }


        //var button = HeadLayoutButtonRow?. .FirstOrDefault(a => a.Text.Trim() == result.MessageText)
        //            ?? SubHeadLayoutButtonRow?.FirstOrDefault(a => a.Text.Trim() == result.MessageText);

        // bf.ToList().FirstOrDefault(a => a.Text.Trim() == result.MessageText)

        //var index = bf.FindRowByButton(button);



        check:


            //Remove button click message
            if (this.DeleteReplyMessage)
                await Device.DeleteMessage(result.MessageId);

            if (match == null)
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
                else if (result.MessageText.EndsWith(CheckedIconLabel))
                {
                    var s = result.MessageText.Split(' ', '.');
                    index = int.Parse(s[0]) - 1;

                    if (!this.CheckedRows.Contains(index))
                        return;

                    this.CheckedRows.Remove(index);

                    this.Updated();

                    await OnCheckedChanged(new CheckedChangedEventArgs(ButtonsForm[index], index, false));

                }
                else if (result.MessageText.EndsWith(UncheckedIconLabel))
                {
                    var s = result.MessageText.Split(' ', '.');
                    index = int.Parse(s[0]) - 1;

                    if (this.CheckedRows.Contains(index))
                        return;


                    this.CheckedRows.Add(index);

                    this.Updated();

                    await OnCheckedChanged(new CheckedChangedEventArgs(ButtonsForm[index], index, true));

                }
                //else if (this.EnableSearch)
                //{
                //    if (result.MessageText.StartsWith("🔍"))
                //    {
                //        //Sent note about searching
                //        if (this.SearchQuery == null)
                //        {
                //            await this.Device.Send(this.SearchLabel);
                //        }

                //        this.SearchQuery = null;
                //        this.Updated();
                //        return;
                //    }

                //    this.SearchQuery = result.MessageText;

                //    if (this.SearchQuery != null && this.SearchQuery != "")
                //    {
                //        this.CurrentPageIndex = 0;
                //        this.Updated();
                //    }

                //}



                return;
            }


            await OnButtonClicked(new ButtonClickedEventArgs(match.GetButtonMatch(result.MessageText), index, match));

            result.Handled = true;

            //await OnButtonClicked(new ButtonClickedEventArgs(button, index));

            ////Remove button click message
            //if (this.DeletePreviousMessage)
            //    await Device.DeleteMessage(result.MessageId);

            //result.Handled = true;

        }

        public async override Task Action(MessageResult result, string value = null)
        {
            if (result.Handled)
                return;

            if (!result.IsFirstHandler)
                return;

            //Find clicked button depending on Text or Value (depending on markup type)
            if (this.KeyboardType != eKeyboardType.InlineKeyBoard)
                return;

            await result.ConfirmAction(this.ConfirmationText ?? "");

            ButtonRow match = null;
            int index = -1;

            if (HeadLayoutButtonRow?.Matches(result.RawData, false) ?? false)
            {
                match = HeadLayoutButtonRow;
                goto check;
            }

            if (SubHeadLayoutButtonRow?.Matches(result.RawData, false) ?? false)
            {
                match = SubHeadLayoutButtonRow;
                goto check;
            }

            var br = DataSource.FindRow(result.RawData, false);
            if (br != null)
            {
                match = br.Item1;
                index = br.Item2;
            }



        //var bf = DataSource.ButtonForm;

        //var button = HeadLayoutButtonRow?.FirstOrDefault(a => a.Value == result.RawData)
        //            ?? SubHeadLayoutButtonRow?.FirstOrDefault(a => a.Value == result.RawData)
        //            ?? bf.ToList().FirstOrDefault(a => a.Value == result.RawData);

        //var index = bf.FindRowByButton(button);

        check:
            if (match != null)
            {
                await OnButtonClicked(new ButtonClickedEventArgs(match.GetButtonMatch(result.RawData, false), index, match));

                result.Handled = true;
                return;
            }

            //if (button != null)
            //{
            //    await OnButtonClicked(new ButtonClickedEventArgs(button, index));

            //    result.Handled = true;
            //    return;
            //}

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

                default:

                    var s = result.RawData.Split('$');


                    switch (s[0])
                    {
                        case "check":

                            index = int.Parse(s[1]);

                            if (!this.CheckedRows.Contains(index))
                            {
                                this.CheckedRows.Add(index);

                                this.Updated();

                                await OnCheckedChanged(new CheckedChangedEventArgs(ButtonsForm[index], index, true));
                            }

                            break;

                        case "uncheck":

                            index = int.Parse(s[1]);

                            if (this.CheckedRows.Contains(index))
                            {
                                this.CheckedRows.Remove(index);

                                this.Updated();

                                await OnCheckedChanged(new CheckedChangedEventArgs(ButtonsForm[index], index, false));
                            }

                            break;
                    }


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

                    if (DataSource.RowCount > Constants.Telegram.MaxInlineKeyBoardRows && !this.EnablePaging)
                    {
                        throw new MaximumRowsReachedException() { Value = DataSource.RowCount, Maximum = Constants.Telegram.MaxInlineKeyBoardRows };
                    }

                    if (DataSource.ColumnCount > Constants.Telegram.MaxInlineKeyBoardCols)
                    {
                        throw new MaximumColsException() { Value = DataSource.ColumnCount, Maximum = Constants.Telegram.MaxInlineKeyBoardCols };
                    }

                    break;

                case eKeyboardType.ReplyKeyboard:

                    if (DataSource.RowCount > Constants.Telegram.MaxReplyKeyboardRows && !this.EnablePaging)
                    {
                        throw new MaximumRowsReachedException() { Value = DataSource.RowCount, Maximum = Constants.Telegram.MaxReplyKeyboardRows };
                    }

                    if (DataSource.ColumnCount > Constants.Telegram.MaxReplyKeyboardCols)
                    {
                        throw new MaximumColsException() { Value = DataSource.ColumnCount, Maximum = Constants.Telegram.MaxReplyKeyboardCols };
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

            ButtonForm form = this.DataSource.PickItems(CurrentPageIndex * ItemRowsPerPage, ItemRowsPerPage, null);

            //if (this.EnableSearch && this.SearchQuery != null && this.SearchQuery != "")
            //{
            //    form = form.FilterDuplicate(this.SearchQuery, true);
            //}
            //else
            //{
            //form = form.Duplicate();
            //}

            if (this.EnablePaging)
            {
                IntegratePagingView(form);
            }
            else
            {
                form = PrepareCheckableLayout(form);
            }

            if (this.HeadLayoutButtonRow != null && HeadLayoutButtonRow.Count > 0)
            {
                form.InsertButtonRow(0, this.HeadLayoutButtonRow.ToArray());
            }

            if (this.SubHeadLayoutButtonRow != null && SubHeadLayoutButtonRow.Count > 0)
            {
                if (this.IsNavigationBarVisible)
                {
                    form.InsertButtonRow(2, this.SubHeadLayoutButtonRow.ToArray());
                }
                else
                {
                    form.InsertButtonRow(1, this.SubHeadLayoutButtonRow.ToArray());
                }
            }

            switch (this.KeyboardType)
            {
                //Reply Keyboard could only be updated with a new keyboard.
                case eKeyboardType.ReplyKeyboard:

                    if (form.Count == 0)
                    {
                        if (this.MessageId != null)
                        {
                            await this.Device.HideReplyKeyboard();
                            this.MessageId = null;
                        }

                        return;
                    }

                    //if (form.Count == 0)
                    //    return;


                    var rkm = (ReplyKeyboardMarkup)form;
                    rkm.ResizeKeyboard = this.ResizeKeyboard;
                    rkm.OneTimeKeyboard = this.OneTimeKeyboard;
                    m = await this.Device.Send(this.Title, rkm, disableNotification: true, parseMode: MessageParseMode, MarkdownV2AutoEscape: false);

                    //Prevent flicker of keyboard
                    if (this.DeletePreviousMessage && this.MessageId != null)
                        await this.Device.DeleteMessage(this.MessageId.Value);

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

        private void IntegratePagingView(ButtonForm dataForm)
        {
            //No Items 
            if (dataForm.Rows == 0)
            {
                dataForm.AddButtonRow(new ButtonBase(NoItemsLabel, "$"));
            }

            ButtonForm bf = new ButtonForm();

            bf = PrepareCheckableLayout(dataForm);


            if (this.IsNavigationBarVisible)
            {
                //🔍
                ButtonRow row = new ButtonRow();
                row.Add(new ButtonBase(PreviousPageLabel, "$previous$"));
                row.Add(new ButtonBase(String.Format(Localizations.Default.Language["ButtonGrid_CurrentPage"], this.CurrentPageIndex + 1, this.PageCount), "$site$"));
                row.Add(new ButtonBase(NextPageLabel, "$next$"));


                dataForm.InsertButtonRow(0, row);

                dataForm.AddButtonRow(row);
            }
        }

        private ButtonForm PrepareCheckableLayout(ButtonForm dataForm)
        {
            var bf = new ButtonForm();
            for (int i = 0; i < dataForm.Rows; i++)
            {
                int it = (this.CurrentPageIndex * (this.MaximumRow - LayoutRows)) + i;

                //if (it > dataForm.Rows - 1)
                //    break;

                var r = dataForm[i];

                String s = CheckedRows.Contains(it) ? this.CheckedIconLabel : this.UncheckedIconLabel;

                //On reply keyboards we need a unique text.
                if (this.KeyboardType == eKeyboardType.ReplyKeyboard)
                {
                    s = $"{it + 1}. " + s;
                }

                if (CheckedRows.Contains(it))
                {
                    r.Insert(0, new ButtonBase(s, "uncheck$" + it.ToString()));
                }
                else
                {
                    r.Insert(0, new ButtonBase(s, "check$" + it.ToString()));
                }

                bf.AddButtonRow(r);
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
                return this.LayoutRows + DataSource.RowCount;
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

        /// <summary>
        /// Returns the number of item rows per page.
        /// </summary>
        public int ItemRowsPerPage
        {
            get
            {
                return this.MaximumRow - this.LayoutRows;
            }
        }

        public int PageCount
        {
            get
            {
                if (DataSource.RowCount == 0)
                    return 1;

                //var bf = this.DataSource.PickAllItems(this.EnableSearch ? this.SearchQuery : null);

                var max = this.DataSource.RowCount;

                //if (this.EnableSearch && this.SearchQuery != null && this.SearchQuery != "")
                //{
                //    bf = bf.FilterDuplicate(this.SearchQuery);
                //}

                if (max == 0)
                    return 1;

                return (int)Math.Ceiling((decimal)((decimal)max / (decimal)ItemRowsPerPage));
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

        public List<ButtonBase> CheckedItems
        {
            get
            {
                List<ButtonBase> lst = new List<ButtonBase>();

                foreach (var c in CheckedRows)
                {
                    lst.Add(this.ButtonsForm[c][0]);
                }

                return lst;
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
