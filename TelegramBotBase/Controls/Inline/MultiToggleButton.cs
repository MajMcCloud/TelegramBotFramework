using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Localizations;

namespace TelegramBotBase.Controls.Inline;

public class MultiToggleButton : ControlBase
{
    private static readonly object EvToggled = new();

    private readonly EventHandlerList _events = new();

    private bool _renderNecessary = true;


    public MultiToggleButton()
    {
        Options = new List<ButtonBase>();
    }

    /// <summary>
    ///     This contains the selected icon.
    /// </summary>
    public string SelectedIcon { get; set; } = Default.Language["MultiToggleButton_SelectedIcon"];

    /// <summary>
    ///     This will appear on the ConfirmAction message (if not empty)
    /// </summary>
    public string ChangedString { get; set; } = Default.Language["MultiToggleButton_Changed"];

    private string _title = Default.Language["MultiToggleButton_Title"];

    /// <summary>
    ///     This holds the title of the control.
    /// </summary>
    public String Title
    {
        get
        {
            return _title;
        }
        set
        {
            if (_title == value)
                return;

            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException($"{nameof(Title)}", $"{nameof(Title)} must have been a value unequal to null/empty");
            }

            _title = value;
            _renderNecessary = true;

        }
    }

    public int? MessageId { get; set; }

    /// <summary>
    ///     This will hold all options available.
    /// </summary>
    public List<ButtonBase> Options { get; set; }

    /// <summary>
    ///     This will set if an empty selection (null) is allowed.
    /// </summary>
    public bool AllowEmptySelection { get; set; } = true;

    public ButtonBase SelectedOption { get; set; }

    public event EventHandler Toggled
    {
        add => _events.AddHandler(EvToggled, value);
        remove => _events.RemoveHandler(EvToggled, value);
    }

    public void OnToggled(EventArgs e)
    {
        (_events[EvToggled] as EventHandler)?.Invoke(this, e);
    }

    public override async Task Action(MessageResult result, string value = null)
    {
        if (result.Handled)
        {
            return;
        }

        await result.ConfirmAction(ChangedString);

        switch (value ?? "unknown")
        {
            default:

                var s = value.Split('$');

                if (s[0] == "check" && s.Length > 1)
                {
                    var index = 0;
                    if (!int.TryParse(s[1], out index))
                    {
                        return;
                    }

                    if (SelectedOption == null || SelectedOption != Options[index])
                    {
                        SelectedOption = Options[index];
                        OnToggled(EventArgs.Empty);
                    }
                    else if (AllowEmptySelection)
                    {
                        SelectedOption = null;
                        OnToggled(EventArgs.Empty);
                    }

                    _renderNecessary = true;

                    return;
                }


                _renderNecessary = false;

                break;
        }

        result.Handled = true;
    }

    public override async Task Render(MessageResult result)
    {
        if (!_renderNecessary)
        {
            return;
        }

        var bf = new ButtonForm(this);

        var lst = new List<ButtonBase>();
        foreach (var o in Options)
        {
            var index = Options.IndexOf(o);
            if (o == SelectedOption)
            {
                lst.Add(new ButtonBase(SelectedIcon + " " + o.Text, "check$" + index));
                continue;
            }

            lst.Add(new ButtonBase(o.Text, "check$" + index));
        }

        bf.AddButtonRow(lst);

        if (MessageId != null)
        {
            var m = await Device.Edit(MessageId.Value, Title, bf);
        }
        else
        {
            var m = await Device.Send(Title, bf, disableNotification: true);
            if (m != null)
            {
                MessageId = m.MessageId;
            }
        }

        _renderNecessary = false;
    }
}