// ClubMateBot demo - SKILL.md 7.1/7.2 (ButtonGrid + custom ButtonFormDataSource) and the
// generic IDataSource<T>/StaticDataSource<T> helper.
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.DataSources;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace ClubMateBot.Controls;

/// <summary>
///     Projection of a club member that is small enough to live inside a button.
///     Phase M3 swaps the in-memory list behind <see cref="MemberDataSource"/> for the EF Core
///     entity; the button contract (id + display name) does not change.
/// </summary>
/// <param name="Id">Stable identifier used as the button value.</param>
/// <param name="FirstName">Given name.</param>
/// <param name="LastName">Family name.</param>
/// <param name="Category">Membership category, used for the tag filter demo.</param>
/// <param name="IsActive">False for expired or frozen memberships.</param>
public sealed record MemberPreview(string Id, string FirstName, string LastName, string Category, bool IsActive)
{
    public string DisplayName => $"{LastName.ToUpperInvariant()}, {FirstName}";
}

/// <summary>
///     Custom <see cref="ButtonFormDataSource"/>.
///     <para>
///     A <see cref="ButtonGrid"/> never copies the whole data set: it asks
///     <see cref="CalculateMax"/> for the number of rows, then <see cref="PickItems"/> for the rows
///     of the current page only. That is exactly the shape a database needs - a real implementation
///     turns those calls into <c>Count()</c> and <c>Skip()/Take()</c> queries. The counters on this
///     class make the lazy loading visible in the chat.
///     </para>
///     <para>
///     The rows are produced from an <see cref="IDataSource{T}"/>. Here it is the framework's
///     <see cref="StaticDataSource{T}"/> (an in-memory list); in phase M3 it becomes an EF Core
///     backed implementation without touching this class' public shape.
///     </para>
/// </summary>
public sealed class MemberDataSource : ButtonFormDataSource
{
    private readonly IDataSource<MemberPreview> _source;
    private readonly int _columns;

    private int _pagesRequested;
    private int _itemsMaterialized;

    public MemberDataSource(IEnumerable<MemberPreview> members, int columns = 2)
    {
        //StaticDataSource<T> is the generic IDataSource<T> helper shipped by the framework.
        _source = new StaticDataSource<MemberPreview>(members.ToList());
        _columns = Math.Clamp(columns, 1, 3);
    }

    /// <summary>Number of pages the grid has asked for (proof that the grid pages lazily).</summary>
    public int PagesRequested => _pagesRequested;

    /// <summary>Number of members that were actually turned into buttons.</summary>
    public int ItemsMaterialized => _itemsMaterialized;

    /// <summary>Total number of members behind the source.</summary>
    public int TotalMembers => _source.Count;

    //Row based counts: the grid plans its keyboard in rows, not in items.
    public override int Count => _source.Count;

    public override int RowCount => (int)Math.Ceiling(_source.Count / (decimal)_columns);

    public override int ColumnCount => _columns;

    public override ButtonRow ItemAt(int index)
    {
        return RenderRow(index, Filtered(null));
    }

    public override List<ButtonRow> ItemRange(int start, int count)
    {
        var all = Filtered(null);

        return Enumerable.Range(start, Math.Min(count, Math.Max(0, all.Count - start)))
                         .Select(i => RenderRow(i, all))
                         .ToList();
    }

    public override List<ButtonRow> AllItems()
    {
        var all = Filtered(null);

        return Enumerable.Range(0, all.Count).Select(i => RenderRow(i, all)).ToList();
    }

    /// <summary>
    ///     This is the method a paged grid calls for the visible page. <paramref name="start"/> and
    ///     <paramref name="count"/> are row based, <paramref name="filter"/> is the search text.
    /// </summary>
    public override ButtonForm PickItems(int start, int count, string? filter = null)
    {
        var rows = Filtered(filter);

        _pagesRequested++;

        var bf = new ButtonForm();

        for (var i = start; i < start + count && i < rows.Count; i++)
        {
            bf.AddButtonRow(RenderRow(i, rows));
        }

        return bf;
    }

    public override ButtonForm PickAllItems(string? filter = null)
    {
        var rows = Filtered(filter);

        var bf = new ButtonForm();

        for (var i = 0; i < rows.Count; i++)
        {
            bf.AddButtonRow(RenderRow(i, rows));
        }

        return bf;
    }

    /// <summary>
    ///     Row count for the current filter - the grid divides this by the free rows per keyboard to
    ///     get the page count.
    /// </summary>
    public override int CalculateMax(string? filter = null)
    {
        return Filtered(filter).Count;
    }

    /// <summary>
    ///     The grid resolves a click (callback value or reply keyboard text) through this method.
    ///     The base implementation only knows the static <see cref="ButtonFormDataSource.ButtonForm"/>,
    ///     which stays empty here, so the lookup has to be overridden for a lazy source.
    /// </summary>
    public override Tuple<ButtonRow, int>? FindRow(string text, bool useText = true)
    {
        var members = _source.AllItems();

        for (var i = 0; i < members.Count; i++)
        {
            var m = members[i];

            var hit = useText
                          ? m.DisplayName.Equals(text.Trim(), StringComparison.InvariantCultureIgnoreCase)
                          : m.Id.Equals(text, StringComparison.InvariantCultureIgnoreCase);

            if (!hit)
            {
                continue;
            }

            var rowIndex = i / _columns;

            return new Tuple<ButtonRow, int>(RenderRow(rowIndex, Filtered(null)), rowIndex);
        }

        return null;
    }

    /// <summary>Turns one member into one button (the framework calls this for single items).</summary>
    public override ButtonRow Render(object data)
    {
        if (data is not MemberPreview m)
        {
            return new ButtonRow(new ButtonBase("(empty)", "none"));
        }

        return new ButtonRow(RenderButton(m));
    }

    private ButtonRow RenderRow(int rowIndex, List<MemberPreview> members)
    {
        var row = new ButtonRow();

        for (var c = 0; c < _columns; c++)
        {
            var index = rowIndex * _columns + c;

            if (index >= members.Count)
            {
                break;
            }

            row.Add(RenderButton(members[index]));
        }

        return row;
    }

    private ButtonBase RenderButton(MemberPreview m)
    {
        _itemsMaterialized++;

        var text = m.IsActive ? m.DisplayName : $"{m.DisplayName} ⛔";

        return new ButtonBase(text, m.Id);
    }

    private List<MemberPreview> Filtered(string? filter)
    {
        var all = _source.AllItems();

        if (string.IsNullOrWhiteSpace(filter))
        {
            return all;
        }

        return all.Where(a => a.DisplayName.Contains(filter.Trim(), StringComparison.InvariantCultureIgnoreCase)
                             || a.Category.Contains(filter.Trim(), StringComparison.InvariantCultureIgnoreCase))
                  .ToList();
    }
}
