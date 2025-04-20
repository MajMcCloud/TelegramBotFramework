using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotBase.Base;
using TelegramBotBase.Controls.Hybrid;

namespace TelegramBotBase.Form;

/// <summary>
///     Base class for an buttons array
/// </summary>
public class ButtonForm
{
    private readonly List<ButtonRow> _buttons = new();

    public ButtonForm()
    {
    }

    public ButtonForm(ControlBase control)
    {
        DependencyControl = control;
    }

    public ButtonForm(IEnumerable<ButtonRow> rows)
    {
        _buttons = rows.ToList();
    }


    public IReplyMarkup Markup { get; set; }

    public ControlBase DependencyControl { get; set; }

    /// <summary>
    ///     Contains the number of rows.
    /// </summary>
    public int Rows => _buttons.Count;

    /// <summary>
    ///     Contains the highest number of columns in an row.
    /// </summary>
    public int Cols
    {
        get { return _buttons.Select(a => a.Count).OrderByDescending(a => a).FirstOrDefault(); }
    }


    public ButtonRow this[int row] => _buttons[row];

    public int Count
    {
        get
        {
            if (_buttons.Count == 0)
            {
                return 0;
            }

            return _buttons.Select(a => a.ToArray()).ToList().Aggregate((a, b) => a.Union(b).ToArray()).Length;
        }
    }

    public void AddButtonRow(string text, string value, string url = null)
    {
        _buttons.Add(new List<ButtonBase> { new(text, value, url) });
    }

    //public void AddButtonRow(ButtonRow row)
    //{
    //    Buttons.Add(row.ToList());
    //}

    public void AddButtonRow(ButtonRow row)
    {
        _buttons.Add(row);
    }

    public void AddButtonRow(params ButtonBase[] row)
    {
        AddButtonRow(row.ToList());
    }

    public void AddButtonRows(IEnumerable<ButtonRow> rows)
    {
        _buttons.AddRange(rows);
    }

    public void InsertButtonRow(int index, IEnumerable<ButtonBase> row)
    {
        _buttons.Insert(index, row.ToList());
    }

    public void InsertButtonRow(int index, ButtonRow row)
    {
        _buttons.Insert(index, row);
    }

    //public void InsertButtonRow(int index, params ButtonBase[] row)
    //{
    //    InsertButtonRow(index, row.ToList());
    //}

    public static T[][] SplitTo<T>(IEnumerable<T> items, int itemsPerRow = 2)
    {
        var splitted = default(T[][]);

        try
        {
            var t = items.Select((a, index) => new { a, index })
                         .GroupBy(a => a.index / itemsPerRow)
                         .Select(a => a.Select(b => b.a).ToArray()).ToArray();

            splitted = t;
        }
        catch
        {
        }

        return splitted;
    }

    /// <summary>
    ///     Add buttons splitted in the amount of columns (i.e. 2 per row...)
    /// </summary>
    /// <param name="buttons"></param>
    /// <param name="buttonsPerRow"></param>
    public void AddSplitted(IEnumerable<ButtonBase> buttons, int buttonsPerRow = 2)
    {
        var sp = SplitTo(buttons, buttonsPerRow);

        foreach (var bl in sp)
        {
            AddButtonRow(bl);
        }
    }

    /// <summary>
    ///     Returns a range of rows from the buttons.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public List<ButtonRow> GetRange(int start, int count)
    {
        return _buttons.Skip(start).Take(count).ToList();
    }


    public List<ButtonBase> ToList()
    {
        return _buttons.DefaultIfEmpty(new List<ButtonBase>()).Select(a => a.ToList())
                       .Aggregate((a, b) => a.Union(b).ToList());
    }

    public List<ButtonRow> ToRowList()
    {
        return _buttons;
    }

    public InlineKeyboardButton[][] ToInlineButtonArray()
    {
        var ikb = _buttons.Select(a => a.ToArray().Select(b => b.ToInlineButton(this)).ToArray()).ToArray();

        return ikb;
    }

    public KeyboardButton[][] ToReplyButtonArray()
    {
        var ikb = _buttons.Select(a => a.ToArray().Select(b => b.ToKeyboardButton(this)).ToArray()).ToArray();

        return ikb;
    }

    public List<ButtonRow> ToArray()
    {
        return _buttons;
    }

    public int FindRowByButton(ButtonBase button)
    {
        var row = _buttons.FirstOrDefault(a => a.ToArray().Count(b => b == button) > 0);
        if (row == null)
        {
            return -1;
        }

        return _buttons.IndexOf(row);
    }

    public Tuple<ButtonRow, int> FindRow(string text, bool useText = true)
    {
        var r = _buttons.FirstOrDefault(a => a.Matches(text, useText));
        if (r == null)
        {
            return null;
        }

        var i = _buttons.IndexOf(r);
        return new Tuple<ButtonRow, int>(r, i);
    }


    /// <summary>
    ///     Returns the first Button with the given value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public ButtonBase GetButtonByValue(string value)
    {
        return ToList().Where(a => a.Value == value).FirstOrDefault();
    }

    public static implicit operator InlineKeyboardMarkup(ButtonForm form)
    {
        if (form == null)
        {
            return null;
        }

        var ikm = new InlineKeyboardMarkup(form.ToInlineButtonArray());

        return ikm;
    }

    public static implicit operator ReplyKeyboardMarkup(ButtonForm form)
    {
        if (form == null)
        {
            return null;
        }

        var ikm = new ReplyKeyboardMarkup(form.ToReplyButtonArray());

        return ikm;
    }

    /// <summary>
    ///     Creates a copy of this form.
    /// </summary>
    /// <returns></returns>
    public ButtonForm Duplicate()
    {
        var bf = new ButtonForm
        {
            Markup = Markup,
            DependencyControl = DependencyControl
        };

        foreach (var b in _buttons)
        {
            var lst = new ButtonRow();
            foreach (var b2 in b)
            {
                lst.Add(b2);
            }

            bf._buttons.Add(lst);
        }

        return bf;
    }

    /// <summary>
    ///     Creates a copy of this form and filters by the parameter.
    /// </summary>
    /// <returns></returns>
    public ButtonForm FilterDuplicate(string filter, bool byRow = false)
    {
        var bf = new ButtonForm
        {
            Markup = Markup,
            DependencyControl = DependencyControl
        };

        foreach (var b in _buttons)
        {
            var lst = new ButtonRow();
            foreach (var b2 in b)
            {
                if (b2.Text.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) == -1)
                {
                    continue;
                }

                //Copy full row, when at least one match has found.
                if (byRow)
                {
                    lst = b;
                    break;
                }

                lst.Add(b2);
            }

            if (lst.Count > 0)
            {
                bf._buttons.Add(lst);
            }
        }

        return bf;
    }

    /// <summary>
    ///     Creates a copy of this form and filters by the parameter.
    /// </summary>
    /// <returns></returns>
    public ButtonForm TagDuplicate(List<string> tags, bool byRow = false)
    {
        var bf = new ButtonForm
        {
            Markup = Markup,
            DependencyControl = DependencyControl
        };

        foreach (var b in _buttons)
        {
            var lst = new ButtonRow();
            foreach (var b2 in b)
            {
                if (!(b2 is TagButtonBase tb))
                {
                    continue;
                }

                if (!tags.Contains(tb.Tag))
                {
                    continue;
                }

                //Copy full row, when at least one match has found.
                if (byRow)
                {
                    lst = b;
                    break;
                }

                lst.Add(b2);
            }

            if (lst.Count > 0)
            {
                bf._buttons.Add(lst);
            }
        }

        return bf;
    }
}