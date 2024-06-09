using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.DataSources;
using TelegramBotBase.Enums;
using TelegramBotBase.Exceptions;
using TelegramBotBase.Form;
using TelegramBotBase.Localizations;
using static System.Net.Mime.MediaTypeNames;
using static TelegramBotBase.Base.Async;

namespace TelegramBotBase.Controls.Hybrid;

public class CheckedButtonList : ControlBase
{
    private static readonly object EvButtonClicked = new();

    private static readonly object EvCheckedChanged = new();

    private readonly EventHandlerList _events = new();

    private EKeyboardType _mEKeyboardType = EKeyboardType.ReplyKeyboard;

    private bool _renderNecessary = true;

    public string NextPageLabel = Default.Language["ButtonGrid_NextPage"];

    public string NoItemsLabel = Default.Language["ButtonGrid_NoItems"];

    public string PreviousPageLabel = Default.Language["ButtonGrid_PreviousPage"];

    public CheckedButtonList()
    {
        DataSource = new ButtonFormDataSource();
    }

    public CheckedButtonList(EKeyboardType type) : this()
    {
        _mEKeyboardType = type;
    }


    public CheckedButtonList(ButtonForm form)
    {
        DataSource = new ButtonFormDataSource(form);
    }

    string m_Title = Default.Language["ButtonGrid_Title"];

    public string Title
    {
        get
        {
            return m_Title;
        }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException($"{nameof(Title)}", $"{nameof(Title)} property must have been a value unequal to null/empty");
            }

            m_Title = value;
        }
    }

    public string ConfirmationText { get; set; } = "";

    /// <summary>
    ///     Data source of the items.
    /// </summary>
    public ButtonFormDataSource DataSource { get; set; }

    private List<int> CheckedRows { get; } = new();

    public int? MessageId { get; set; }


    /// <summary>
    ///     Optional. Requests clients to resize the keyboard vertically for optimal fit (e.g., make the keyboard smaller if
    ///     there are just two rows of buttons). Defaults to false, in which case the custom keyboard is always of the same
    ///     height as the app's standard keyboard.
    ///     Source: https://core.telegram.org/bots/api#replykeyboardmarkup
    /// </summary>
    public bool ResizeKeyboard { get; set; } = false;

    public bool OneTimeKeyboard { get; set; } = false;

    public bool HideKeyboardOnCleanup { get; set; } = true;

    public bool DeletePreviousMessage { get; set; } = true;

    /// <summary>
    ///     Removes the reply message from a user.
    /// </summary>
    public bool DeleteReplyMessage { get; set; } = true;

    /// <summary>
    ///     Parsemode of the message.
    /// </summary>
    public ParseMode MessageParseMode { get; set; } = ParseMode.Markdown;

    /// <summary>
    ///     Enables automatic paging of buttons when the amount of rows is exceeding the limits.
    /// </summary>
    public bool EnablePaging { get; set; } = false;


    ///// <summary>
    ///// Enabled a search function.
    ///// </summary>
    //public bool EnableSearch { get; set; } = false;

    //public String SearchQuery { get; set; }

    public ENavigationBarVisibility NavigationBarVisibility { get; set; } = ENavigationBarVisibility.always;


    /// <summary>
    ///     Index of the current page
    /// </summary>
    public int CurrentPageIndex { get; set; }

    //public String SearchLabel = Localizations.Default.Language["ButtonGrid_SearchFeature"];

    public string CheckedIconLabel { get; set; } = "✅";

    public string UncheckedIconLabel { get; set; } = "◻️";

    /// <summary>
    ///     Layout of the buttons which should be displayed always on top.
    /// </summary>
    public ButtonRow HeadLayoutButtonRow { get; set; }

    /// <summary>
    ///     Layout of columns which should be displayed below the header
    /// </summary>
    public ButtonRow SubHeadLayoutButtonRow { get; set; }

    /// <summary>
    ///     Defines which type of Button Keyboard should be rendered.
    /// </summary>
    public EKeyboardType KeyboardType
    {
        get => _mEKeyboardType;
        set
        {
            if (_mEKeyboardType != value)
            {
                _renderNecessary = true;

                Cleanup().Wait();

                _mEKeyboardType = value;
            }
        }
    }

    public bool PagingNecessary
    {
        get
        {
            if (KeyboardType == EKeyboardType.InlineKeyBoard &&
                TotalRows > Constants.Telegram.MaxInlineKeyBoardRows)
            {
                return true;
            }

            if (KeyboardType == EKeyboardType.ReplyKeyboard &&
                TotalRows > Constants.Telegram.MaxReplyKeyboardRows)
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
            if ((NavigationBarVisibility == ENavigationBarVisibility.always) |
                (NavigationBarVisibility == ENavigationBarVisibility.auto && PagingNecessary))
            {
                return true;
            }

            return false;
        }
    }

    /// <summary>
    ///     Returns the maximum number of rows
    /// </summary>
    public int MaximumRow
    {
        get
        {
            return KeyboardType switch
            {
                EKeyboardType.InlineKeyBoard => Constants.Telegram.MaxInlineKeyBoardRows,
                EKeyboardType.ReplyKeyboard => Constants.Telegram.MaxReplyKeyboardRows,
                _ => 0
            };
        }
    }

    /// <summary>
    ///     Returns the number of all rows (layout + navigation + content);
    /// </summary>
    public int TotalRows => LayoutRows + DataSource.RowCount;


    /// <summary>
    ///     Contains the Number of Rows which are used by the layout.
    /// </summary>
    private int LayoutRows
    {
        get
        {
            var layoutRows = 0;

            if ((NavigationBarVisibility == ENavigationBarVisibility.always) |
                (NavigationBarVisibility == ENavigationBarVisibility.auto))
            {
                layoutRows += 2;
            }

            if (HeadLayoutButtonRow != null && HeadLayoutButtonRow.Count > 0)
            {
                layoutRows++;
            }

            if (SubHeadLayoutButtonRow != null && SubHeadLayoutButtonRow.Count > 0)
            {
                layoutRows++;
            }

            return layoutRows;
        }
    }

    /// <summary>
    ///     Returns the number of item rows per page.
    /// </summary>
    public int ItemRowsPerPage => MaximumRow - LayoutRows;

    public int PageCount
    {
        get
        {
            if (DataSource.RowCount == 0)
            {
                return 1;
            }

            //var bf = this.DataSource.PickAllItems(this.EnableSearch ? this.SearchQuery : null);

            var max = DataSource.RowCount;

            //if (this.EnableSearch && this.SearchQuery != null && this.SearchQuery != "")
            //{
            //    bf = bf.FilterDuplicate(this.SearchQuery);
            //}

            if (max == 0)
            {
                return 1;
            }

            return (int)Math.Ceiling(max / (decimal)ItemRowsPerPage);
        }
    }

    public List<ButtonBase> CheckedItems
    {
        get
        {
            var lst = new List<ButtonBase>();

            foreach (var c in CheckedRows)
            {
                lst.Add(DataSource.ButtonForm[c][0]);
            }

            return lst;
        }
    }

    public event AsyncEventHandler<ButtonClickedEventArgs> ButtonClicked
    {
        add => _events.AddHandler(EvButtonClicked, value);
        remove => _events.RemoveHandler(EvButtonClicked, value);
    }

    public async Task OnButtonClicked(ButtonClickedEventArgs e)
    {
        var handler = _events[EvButtonClicked]?.GetInvocationList()
                                              .Cast<AsyncEventHandler<ButtonClickedEventArgs>>();
        if (handler == null)
        {
            return;
        }

        foreach (var h in handler)
        {
            await h.InvokeAllAsync(this, e);
        }
    }

    public event AsyncEventHandler<CheckedChangedEventArgs> CheckedChanged
    {
        add => _events.AddHandler(EvCheckedChanged, value);
        remove => _events.RemoveHandler(EvCheckedChanged, value);
    }

    public async Task OnCheckedChanged(CheckedChangedEventArgs e)
    {
        var handler = _events[EvCheckedChanged]?.GetInvocationList()
                                               .Cast<AsyncEventHandler<CheckedChangedEventArgs>>();
        if (handler == null)
        {
            return;
        }

        foreach (var h in handler)
        {
            await h.InvokeAllAsync(this, e);
        }
    }

    public override async Task Load(MessageResult result)
    {
        if (KeyboardType != EKeyboardType.ReplyKeyboard)
        {
            return;
        }

        if (!result.IsFirstHandler)
        {
            return;
        }

        if (result.MessageText == null || result.MessageText == "")
        {
            return;
        }

        var matches = new List<ButtonRow>();
        ButtonRow match = null;
        var index = -1;

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
        if (DeleteReplyMessage)
        {
            await Device.DeleteMessage(result.MessageId);
        }

        if (match == null)
        {
            if (result.MessageText == PreviousPageLabel)
            {
                if (CurrentPageIndex > 0)
                {
                    CurrentPageIndex--;
                }

                Updated();
            }
            else if (result.MessageText == NextPageLabel)
            {
                if (CurrentPageIndex < PageCount - 1)
                {
                    CurrentPageIndex++;
                }

                Updated();
            }
            else if (result.MessageText.EndsWith(CheckedIconLabel))
            {
                var s = result.MessageText.Split(' ', '.');
                index = int.Parse(s[0]) - 1;

                if (!CheckedRows.Contains(index))
                {
                    return;
                }

                CheckedRows.Remove(index);

                Updated();

                await OnCheckedChanged(new CheckedChangedEventArgs(DataSource.ButtonForm[index], index, false));
            }
            else if (result.MessageText.EndsWith(UncheckedIconLabel))
            {
                var s = result.MessageText.Split(' ', '.');
                index = int.Parse(s[0]) - 1;

                if (CheckedRows.Contains(index))
                {
                    return;
                }


                CheckedRows.Add(index);

                Updated();

                await OnCheckedChanged(new CheckedChangedEventArgs(DataSource.ButtonForm[index], index, true));
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

    public override async Task Action(MessageResult result, string value = null)
    {
        if (result.Handled)
        {
            return;
        }

        if (!result.IsFirstHandler)
        {
            return;
        }

        //Find clicked button depending on Text or Value (depending on markup type)
        if (KeyboardType != EKeyboardType.InlineKeyBoard)
        {
            return;
        }

        await result.ConfirmAction(ConfirmationText ?? "");

        ButtonRow match = null;
        var index = -1;

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
            await OnButtonClicked(new ButtonClickedEventArgs(match.GetButtonMatch(result.RawData, false), index,
                                                             match));

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

                if (CurrentPageIndex > 0)
                {
                    CurrentPageIndex--;
                }

                Updated();

                break;
            case "$next$":

                if (CurrentPageIndex < PageCount - 1)
                {
                    CurrentPageIndex++;
                }

                Updated();

                break;

            default:

                var s = result.RawData.Split('$');


                switch (s[0])
                {
                    case "check":

                        index = int.Parse(s[1]);

                        if (!CheckedRows.Contains(index))
                        {
                            CheckedRows.Add(index);

                            Updated();

                            await OnCheckedChanged(new CheckedChangedEventArgs(DataSource.ButtonForm[index], index, true));
                        }

                        break;

                    case "uncheck":

                        index = int.Parse(s[1]);

                        if (CheckedRows.Contains(index))
                        {
                            CheckedRows.Remove(index);

                            Updated();

                            await OnCheckedChanged(new CheckedChangedEventArgs(DataSource.ButtonForm[index], index, false));
                        }

                        break;
                }


                break;
        }
    }

    /// <summary>
    ///     This method checks of the amount of buttons
    /// </summary>
    private void CheckGrid()
    {
        switch (_mEKeyboardType)
        {
            case EKeyboardType.InlineKeyBoard:

                if (DataSource.RowCount > Constants.Telegram.MaxInlineKeyBoardRows && !EnablePaging)
                {
                    throw new MaximumRowsReachedException
                        { Value = DataSource.RowCount, Maximum = Constants.Telegram.MaxInlineKeyBoardRows };
                }

                if (DataSource.ColumnCount > Constants.Telegram.MaxInlineKeyBoardCols)
                {
                    throw new MaximumColsException
                        { Value = DataSource.ColumnCount, Maximum = Constants.Telegram.MaxInlineKeyBoardCols };
                }

                break;

            case EKeyboardType.ReplyKeyboard:

                if (DataSource.RowCount > Constants.Telegram.MaxReplyKeyboardRows && !EnablePaging)
                {
                    throw new MaximumRowsReachedException
                        { Value = DataSource.RowCount, Maximum = Constants.Telegram.MaxReplyKeyboardRows };
                }

                if (DataSource.ColumnCount > Constants.Telegram.MaxReplyKeyboardCols)
                {
                    throw new MaximumColsException
                        { Value = DataSource.ColumnCount, Maximum = Constants.Telegram.MaxReplyKeyboardCols };
                }

                break;
        }
    }

    public override async Task Render(MessageResult result)
    {
        if (!_renderNecessary)
        {
            return;
        }

        //Check for rows and column limits
        CheckGrid();

        _renderNecessary = false;

        Message m = null;

        var form = DataSource.PickItems(CurrentPageIndex * ItemRowsPerPage, ItemRowsPerPage);

        //if (this.EnableSearch && this.SearchQuery != null && this.SearchQuery != "")
        //{
        //    form = form.FilterDuplicate(this.SearchQuery, true);
        //}
        //else
        //{
        //form = form.Duplicate();
        //}

        if (EnablePaging)
        {
            IntegratePagingView(form);
        }
        else
        {
            form = PrepareCheckableLayout(form);
        }

        if (HeadLayoutButtonRow != null && HeadLayoutButtonRow.Count > 0)
        {
            form.InsertButtonRow(0, HeadLayoutButtonRow.ToArray());
        }

        if (SubHeadLayoutButtonRow != null && SubHeadLayoutButtonRow.Count > 0)
        {
            if (IsNavigationBarVisible)
            {
                form.InsertButtonRow(2, SubHeadLayoutButtonRow.ToArray());
            }
            else
            {
                form.InsertButtonRow(1, SubHeadLayoutButtonRow.ToArray());
            }
        }

        switch (KeyboardType)
        {
            //Reply Keyboard could only be updated with a new keyboard.
            case EKeyboardType.ReplyKeyboard:

                if (form.Count == 0)
                {
                    if (MessageId != null)
                    {
                        await Device.HideReplyKeyboard();
                        MessageId = null;
                    }

                    return;
                }

                //if (form.Count == 0)
                //    return;


                var rkm = (ReplyKeyboardMarkup)form;
                rkm.ResizeKeyboard = ResizeKeyboard;
                rkm.OneTimeKeyboard = OneTimeKeyboard;
                m = await Device.Send(Title, rkm, disableNotification: true, parseMode: MessageParseMode,
                                      markdownV2AutoEscape: false);

                //Prevent flicker of keyboard
                if (DeletePreviousMessage && MessageId != null)
                {
                    await Device.DeleteMessage(MessageId.Value);
                }

                break;

            case EKeyboardType.InlineKeyBoard:

                if (MessageId != null)
                {
                    m = await Device.Edit(MessageId.Value, Title, (InlineKeyboardMarkup)form);
                }
                else
                {
                    m = await Device.Send(Title, (InlineKeyboardMarkup)form, disableNotification: true,
                                          parseMode: MessageParseMode, markdownV2AutoEscape: false);
                }

                break;
        }

        if (m != null)
        {
            MessageId = m.MessageId;
        }
    }

    private void IntegratePagingView(ButtonForm dataForm)
    {
        //No Items
        if (dataForm.Rows == 0)
        {
            dataForm.AddButtonRow(new ButtonBase(NoItemsLabel, "$"));
        }

        var bf = new ButtonForm();

        bf = PrepareCheckableLayout(dataForm);


        if (IsNavigationBarVisible)
        {
            //🔍
            var row = new ButtonRow();
            row.Add(new ButtonBase(PreviousPageLabel, "$previous$"));
            row.Add(new ButtonBase(
                        string.Format(Default.Language["ButtonGrid_CurrentPage"], CurrentPageIndex + 1, PageCount),
                        "$site$"));
            row.Add(new ButtonBase(NextPageLabel, "$next$"));


            dataForm.InsertButtonRow(0, row);

            dataForm.AddButtonRow(row);
        }
    }

    private ButtonForm PrepareCheckableLayout(ButtonForm dataForm)
    {
        var bf = new ButtonForm();
        for (var i = 0; i < dataForm.Rows; i++)
        {
            var it = CurrentPageIndex * (MaximumRow - LayoutRows) + i;

            //if (it > dataForm.Rows - 1)
            //    break;

            var r = dataForm[i];

            var s = CheckedRows.Contains(it) ? CheckedIconLabel : UncheckedIconLabel;

            //On reply keyboards we need a unique text.
            if (KeyboardType == EKeyboardType.ReplyKeyboard)
            {
                s = $"{it + 1}. " + s;
            }

            if (CheckedRows.Contains(it))
            {
                r.Insert(0, new ButtonBase(s, "uncheck$" + it));
            }
            else
            {
                r.Insert(0, new ButtonBase(s, "check$" + it));
            }

            bf.AddButtonRow(r);
        }

        return bf;
    }

    public override Task Hidden(bool formClose)
    {
        //Prepare for opening Modal, and comming back
        if (!formClose)
        {
            Updated();
        }

        return Task.CompletedTask;
    }


    /// <summary>
    ///     Tells the control that it has been updated.
    /// </summary>
    public void Updated()
    {
        _renderNecessary = true;
    }

    public override async Task Cleanup()
    {
        if (MessageId == null)
        {
            return;
        }

        switch (KeyboardType)
        {
            case EKeyboardType.InlineKeyBoard:

                await Device.DeleteMessage(MessageId.Value);

                MessageId = null;

                break;
            case EKeyboardType.ReplyKeyboard:

                if (HideKeyboardOnCleanup)
                {
                    await Device.HideReplyKeyboard();
                }

                MessageId = null;

                break;
        }
    }
}
