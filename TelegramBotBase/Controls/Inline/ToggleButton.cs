using System;
using System.ComponentModel;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Localizations;

namespace TelegramBotBase.Controls.Inline;

public class ToggleButton : ControlBase
{
    private static readonly object EvToggled = new();

    private readonly EventHandlerList _events = new();

    private bool _renderNecessary = true;


    public ToggleButton()
    {
    }

    public ToggleButton(string checkedString, string uncheckedString)
    {
        CheckedString = checkedString;
        UncheckedString = uncheckedString;
    }

    public string UncheckedIcon { get; set; } = Default.Language["ToggleButton_OffIcon"];

    public string CheckedIcon { get; set; } = Default.Language["ToggleButton_OnIcon"];

    public string CheckedString { get; set; } = Default.Language["ToggleButton_On"];

    public string UncheckedString { get; set; } = Default.Language["ToggleButton_Off"];

    public string ChangedString { get; set; } = Default.Language["ToggleButton_Changed"];

    private string _title = Default.Language["ToggleButton_Title"];

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
                throw new ArgumentNullException($"{nameof(Title)}", $"{nameof(Title)} property must have been a value unequal to null/empty");
            }

            _title = value;
            _renderNecessary = true;

        }
    }



    public int? MessageId { get; set; }

    public bool Checked { get; set; }

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
            case "on":

                if (Checked)
                {
                    return;
                }

                _renderNecessary = true;

                Checked = true;

                OnToggled(EventArgs.Empty);

                break;

            case "off":

                if (!Checked)
                {
                    return;
                }

                _renderNecessary = true;

                Checked = false;

                OnToggled(EventArgs.Empty);

                break;

            default:

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

        var bOn = new ButtonBase((Checked ? CheckedIcon : UncheckedIcon) + " " + CheckedString, "on");

        var bOff = new ButtonBase((!Checked ? CheckedIcon : UncheckedIcon) + " " + UncheckedString, "off");

        bf.AddButtonRow(bOn, bOff);

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