using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests.Controls;

public class ButtonGridForm : AutoCleanForm
{
    private ButtonGrid _mButtons;

    public ButtonGridForm()
    {
        DeleteMode = EDeleteMode.OnLeavingForm;

        Init += ButtonGridForm_Init;
    }

    private Task ButtonGridForm_Init(object sender, InitEventArgs e)
    {
        _mButtons = new ButtonGrid
        {
            KeyboardType = EKeyboardType.InlineKeyBoard
        };

        var bf = new ButtonForm();

        bf.AddButtonRow(new ButtonBase("Back", "back"), new ButtonBase("Switch Keyboard", "switch"));

        bf.AddButtonRow(new ButtonBase("Button1", "b1"), new ButtonBase("Button2", "b2"));

        bf.AddButtonRow(new ButtonBase("Button3", "b3"), new ButtonBase("Button4", "b4"));

        _mButtons.DataSource.ButtonForm = bf;

        _mButtons.ButtonClicked += Bg_ButtonClicked;

        AddControl(_mButtons);
        return Task.CompletedTask;
    }

    private async Task Bg_ButtonClicked(object sender, ButtonClickedEventArgs e)
    {
        if (e.Button == null)
        {
            return;
        }

        if (e.Button.Value == "back")
        {
            var start = new Menu();
            await NavigateTo(start);
        }
        else if (e.Button.Value == "switch")
        {
            _mButtons.KeyboardType = _mButtons.KeyboardType switch
            {
                EKeyboardType.ReplyKeyboard => EKeyboardType.InlineKeyBoard,
                EKeyboardType.InlineKeyBoard => EKeyboardType.ReplyKeyboard,
                _ => _mButtons.KeyboardType
            };
        }
        else
        {
            await Device.Send($"Button clicked with Text: {e.Button.Text} and Value {e.Button.Value}");
        }
    }
}
