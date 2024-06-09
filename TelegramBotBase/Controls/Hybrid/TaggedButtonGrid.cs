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
using static TelegramBotBase.Base.Async;

namespace TelegramBotBase.Controls.Hybrid;

public class TaggedButtonGrid : MultiView
{
    private static readonly object EvButtonClicked = new();

    private readonly EventHandlerList _events = new();

    private EKeyboardType _mEKeyboardType = EKeyboardType.ReplyKeyboard;

    private bool _renderNecessary = true;

    public string BackLabel = Default.Language["ButtonGrid_Back"];

    public string NextPageLabel = Default.Language["ButtonGrid_NextPage"];

    public string NoItemsLabel = Default.Language["ButtonGrid_NoItems"];

    public string PreviousPageLabel = Default.Language["ButtonGrid_PreviousPage"];

    public string SearchLabel = Default.Language["ButtonGrid_SearchFeature"];

    public string TotalTagsLabel = Default.Language["TaggedButtonGrid_TotalTags"];

    public string CheckedTagsLabel = Default.Language["TaggedButtonGrid_CheckedTags"];

    public string CheckAllLabel = Default.Language["TaggedButtonGrid_CheckAll"];

    public string UncheckAllLabel = Default.Language["TaggedButtonGrid_UncheckAll"];

    public string SearchIcon = Default.Language["ButtonGrid_SearchIcon"];

    public string TagIcon = Default.Language["TaggedButtonGrid_TagIcon"];

    public TaggedButtonGrid()
    {
        DataSource = new ButtonFormDataSource();

        SelectedViewIndex = 0;
    }

    public TaggedButtonGrid(EKeyboardType type) : this()
    {
        _mEKeyboardType = type;
    }


    public TaggedButtonGrid(ButtonForm form)
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

    public string ConfirmationText { get; set; }

    /// <summary>
    ///     Data source of the items.
    /// </summary>
    public ButtonFormDataSource DataSource { get; set; }

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

    /// <summary>
    ///     Shows un-/check all tags options
    /// </summary>
    public bool EnableCheckAllTools { get; set; } = false;

    /// <summary>
    ///     Enabled a search function.
    /// </summary>
    public bool EnableSearch { get; set; } = false;

    public string SearchQuery { get; set; }

    public ENavigationBarVisibility NavigationBarVisibility { get; set; } = ENavigationBarVisibility.always;


    /// <summary>
    ///     Index of the current page
    /// </summary>
    public int CurrentPageIndex { get; set; }

    /// <summary>
    ///     Layout of the buttons which should be displayed always on top.
    /// </summary>
    public ButtonRow HeadLayoutButtonRow { get; set; }

    /// <summary>
    ///     Layout of columns which should be displayed below the header
    /// </summary>
    public ButtonRow SubHeadLayoutButtonRow { get; set; }

    /// <summary>
    ///     Layout of columns which should be displayed below the header
    /// </summary>
    private ButtonRow TagsSubHeadLayoutButtonRow { get; set; }

    /// <summary>
    ///     List of Tags which will be allowed to filter by.
    /// </summary>
    public List<string> Tags { get; set; }

    /// <summary>
    ///     List of Tags selected by the User.
    /// </summary>
    public List<string> SelectedTags { get; set; }

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

            if (EnableCheckAllTools && SelectedViewIndex == 1)
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

            var max = DataSource.CalculateMax(EnableSearch ? SearchQuery : null);

            if(SelectedTags.Count < Tags.Count)
            {
                max = DataSource.ButtonForm.TagDuplicate(SelectedTags).Count;
            }

            if (max == 0)
            {
                return 1;
            }

            return (int)Math.Ceiling(max / (decimal)ItemRowsPerPage);
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

    public override void Init()
    {
        Device.MessageDeleted += Device_MessageDeleted;
    }

    private void Device_MessageDeleted(object sender, MessageDeletedEventArgs e)
    {
        if (MessageId == null)
        {
            return;
        }

        if (e.MessageId != MessageId)
        {
            return;
        }

        MessageId = null;
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

        if (TagsSubHeadLayoutButtonRow?.Matches(result.MessageText) ?? false)
        {
            match = TagsSubHeadLayoutButtonRow;
            goto check;
        }

        var br = DataSource.FindRow(result.MessageText);
        if (br != null)
        {
            match = br.Item1;
            index = br.Item2;
        }

        check:


        switch (SelectedViewIndex)
        {
            case 0:

                //Remove button click message
                if (DeleteReplyMessage)
                {
                    await Device.DeleteMessage(result.MessageId);
                }

                if (match != null)
                {
                    await OnButtonClicked(new ButtonClickedEventArgs(match.GetButtonMatch(result.MessageText),
                                                                     index, match));

                    result.Handled = true;
                    return;
                }

                if (result.MessageText == PreviousPageLabel)
                {
                    if (CurrentPageIndex > 0)
                    {
                        CurrentPageIndex--;
                    }

                    Updated();
                    result.Handled = true;
                }
                else if (result.MessageText == NextPageLabel)
                {
                    if (CurrentPageIndex < PageCount - 1)
                    {
                        CurrentPageIndex++;
                    }

                    Updated();
                    result.Handled = true;
                }
                else if (EnableSearch)
                {
                    if (result.MessageText.StartsWith(SearchIcon))
                    {
                        //Sent note about searching
                        if (SearchQuery == null)
                        {
                            await Device.Send(SearchLabel);
                        }

                        SearchQuery = null;
                        Updated();
                        result.Handled = true;
                        return;
                    }

                    SearchQuery = result.MessageText;

                    if (SearchQuery != null && SearchQuery != "")
                    {
                        CurrentPageIndex = 0;
                        Updated();
                        result.Handled = true;
                    }
                }
                else if (Tags != null)
                {
                    if (result.MessageText == TagIcon)
                    {
                        //Remove button click message
                        if (DeletePreviousMessage && !Device.ActiveForm.IsAutoCleanForm())
                        {
                            await Device.DeleteMessage(result.MessageId);
                        }

                        SelectedViewIndex = 1;
                        Updated();
                        result.Handled = true;
                    }
                }

                break;
            case 1:

                //Remove button click message
                if (DeleteReplyMessage)
                {
                    await Device.DeleteMessage(result.MessageId);
                }

                if (result.MessageText == BackLabel)
                {
                    SelectedViewIndex = 0;
                    Updated();
                    result.Handled = true;
                    return;
                }

                if (result.MessageText == CheckAllLabel)
                {
                    CheckAllTags();
                    Updated();
                    result.Handled = true;
                    return;
                }
                else if (result.MessageText == UncheckAllLabel)
                {
                    UncheckAllTags();
                    Updated();
                    result.Handled = true;
                    return;
                }

                var i = result.MessageText.LastIndexOf(" ");
                if (i == -1)
                {
                    i = result.MessageText.Length;
                }

                var t = result.MessageText.Substring(0, i);

                if (SelectedTags.Contains(t))
                {
                    SelectedTags.Remove(t);
                }
                else
                {
                    SelectedTags.Add(t);
                }

                Updated();
                result.Handled = true;


                break;
        }
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

        if (TagsSubHeadLayoutButtonRow?.Matches(result.RawData) ?? false)
        {
            match = TagsSubHeadLayoutButtonRow;
            goto check;
        }

        var br = DataSource.FindRow(result.RawData, false);
        if (br != null)
        {
            match = br.Item1;
            index = br.Item2;
        }

        check:
        if (match != null)
        {
            await OnButtonClicked(new ButtonClickedEventArgs(match.GetButtonMatch(result.RawData, false), index,
                                                             match));

            result.Handled = true;
            return;
        }

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

            case "$filter$":

                SelectedViewIndex = 1;
                Updated();

                break;


            default:

                if (SelectedViewIndex != 1)
                    return;


                switch (result.RawData)
                {
                    case "$back$":

                        SelectedViewIndex = 0;
                        Updated();

                        return;

                    case "$checkall$":

                        CheckAllTags();

                        return;

                    case "$uncheckall$":

                        UncheckAllTags();

                        return;

                }

                if (SelectedTags.Contains(result.RawData))
                {
                    SelectedTags.Remove(result.RawData);
                }
                else
                {
                    SelectedTags.Add(result.RawData);
                }

                Updated();


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

        switch (SelectedViewIndex)
        {
            case 0:

                await RenderDataView();

                break;

            case 1:


                await RenderTagView();


                break;
        }
    }


    #region "Tag View"

    private async Task RenderTagView()
    {
        Message m = null;
        var bf = new ButtonForm();

        bf.AddButtonRow(BackLabel, "$back$");

        if (EnableCheckAllTools)
        {
            TagsSubHeadLayoutButtonRow = new ButtonRow(new ButtonBase(string.Format(TotalTagsLabel, Tags.Count), "$total$"),
                                                        new ButtonBase(CheckAllLabel, "$checkall$"),
                                                        new ButtonBase(UncheckAllLabel, "$uncheckall$"),
                                                        new ButtonBase(string.Format(CheckedTagsLabel, SelectedTags.Count), "checked$"));
            bf.AddButtonRow(TagsSubHeadLayoutButtonRow);
        }

        foreach (var t in Tags)
        {
            var s = t;

            if (SelectedTags?.Contains(t) ?? false)
            {
                s += " ✅";
            }

            bf.AddButtonRow(s, t);
        }

        switch (KeyboardType)
        {
            //Reply Keyboard could only be updated with a new keyboard.
            case EKeyboardType.ReplyKeyboard:

                if (bf.Count == 0)
                {
                    if (MessageId != null)
                    {
                        await Device.HideReplyKeyboard();
                        MessageId = null;
                    }

                    return;
                }

                var rkm = (ReplyKeyboardMarkup)bf;
                rkm.ResizeKeyboard = ResizeKeyboard;
                rkm.OneTimeKeyboard = OneTimeKeyboard;
                m = await Device.Send("Choose category", rkm, disableNotification: true,
                                      parseMode: MessageParseMode, markdownV2AutoEscape: false);

                //Prevent flicker of keyboard
                if (DeletePreviousMessage && MessageId != null)
                {
                    await Device.DeleteMessage(MessageId.Value);
                }

                break;

            case EKeyboardType.InlineKeyBoard:

                if (MessageId != null)
                {
                    m = await Device.Edit(MessageId.Value, "Choose category", (InlineKeyboardMarkup)bf);
                }
                else
                {
                    m = await Device.Send("Choose category", (InlineKeyboardMarkup)bf, disableNotification: true,
                                          parseMode: MessageParseMode, markdownV2AutoEscape: false);
                }

                break;
        }


        if (m != null)
        {
            MessageId = m.MessageId;
        }
    }

    #endregion

    public override Task Hidden(bool formClose)
    {
        //Prepare for opening Modal, and comming back
        if (!formClose)
        {
            Updated();
        }
        else
            //Remove event handler
        {
            Device.MessageDeleted -= Device_MessageDeleted;
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


    /// <summary>
    ///     Checks all tags for filtering.
    /// </summary>
    public void CheckAllTags()
    {
        SelectedTags.Clear();

        SelectedTags = Tags.Select(a => a).ToList();

        Updated();
    }

    /// <summary>
    ///     Unchecks all tags for filtering.
    /// </summary>
    public void UncheckAllTags()
    {
        SelectedTags.Clear();

        Updated();
    }


    #region "Data View"

    private async Task RenderDataView()
    {
        Message m = null;

        ButtonForm form = null;

        if (Tags != null && SelectedTags != null)
        {
            form = DataSource.PickAllItems(EnableSearch ? SearchQuery : null); //CurrentPageIndex * ItemRowsPerPage, ItemRowsPerPage,

            form = form.TagDuplicate(SelectedTags);

            form = new ButtonForm(form.ToRowList().Skip(CurrentPageIndex * ItemRowsPerPage).Take(ItemRowsPerPage));
        }
        else
        {
            form = DataSource.PickItems(CurrentPageIndex * ItemRowsPerPage, ItemRowsPerPage,
                                        EnableSearch ? SearchQuery : null);
        }

        if (EnablePaging)
        {
            IntegratePagingView(form);
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


                //Try to edit message if message id is available
                //When the returned message is null then the message has been already deleted, resend it
                if (MessageId != null)
                {
                    m = await Device.Edit(MessageId.Value, Title, (InlineKeyboardMarkup)form);
                    if (m != null)
                    {
                        MessageId = m.MessageId;
                        return;
                    }
                }

                //When no message id is available or it has been deleted due the use of AutoCleanForm re-render automatically
                m = await Device.Send(Title, (InlineKeyboardMarkup)form, disableNotification: true,
                                      parseMode: MessageParseMode, markdownV2AutoEscape: false);
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

        if (IsNavigationBarVisible)
        {
            //🔍
            var row = new ButtonRow();
            row.Add(new ButtonBase(PreviousPageLabel, "$previous$"));
            row.Add(new ButtonBase(
                        string.Format(Default.Language["ButtonGrid_CurrentPage"], CurrentPageIndex + 1, PageCount),
                        "$site$"));

            if (Tags != null && Tags.Count > 0)
            {
                row.Add(new ButtonBase(TagIcon, "$filter$"));
            }

            row.Add(new ButtonBase(NextPageLabel, "$next$"));

            if (EnableSearch)
            {
                row.Insert(2, new ButtonBase($"{SearchIcon} {(SearchQuery ?? "")}", "$search$"));
            }

            dataForm.InsertButtonRow(0, row);

            dataForm.AddButtonRow(row);
        }
    }

    #endregion
}
