using System.Collections.Generic;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.DataSources;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests.Controls;

public class CheckedButtonListForm : AutoCleanForm
{
    private CheckedButtonList _mButtons;

    public CheckedButtonListForm()
    {
        DeleteMode = EDeleteMode.OnLeavingForm;

        Init += CheckedButtonListForm_Init;
    }

    private Task CheckedButtonListForm_Init(object sender, InitEventArgs e)
    {
        _mButtons = new CheckedButtonList
        {
            KeyboardType = EKeyboardType.InlineKeyBoard,
            EnablePaging = true,
            HeadLayoutButtonRow = new List<ButtonBase> { new("Back", "back"), new("Switch Keyboard", "switch") },
            SubHeadLayoutButtonRow = new List<ButtonBase> { new("No checked items", "$") }
        };

        var bf = new ButtonForm();

        for (var i = 0; i < 30; i++)
        {
            bf.AddButtonRow($"{i + 1}. Item", i.ToString());
        }

        _mButtons.DataSource = new ButtonFormDataSource(bf);

        _mButtons.ButtonClicked += Bg_ButtonClicked;
        _mButtons.CheckedChanged += M_Buttons_CheckedChanged;

        AddControl(_mButtons);
        return Task.CompletedTask;
    }

    private Task M_Buttons_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        _mButtons.SubHeadLayoutButtonRow = new List<ButtonBase>
            { new($"{_mButtons.CheckedItems.Count} checked items", "$") };
        return Task.CompletedTask;
    }

    private async Task Bg_ButtonClicked(object sender, ButtonClickedEventArgs e)
    {
        if (e.Button == null)
        {
            return;
        }

        switch (e.Button.Value)
        {
            case "back":

                var start = new Menu();
                await NavigateTo(start);
                break;


            case "switch":
                _mButtons.KeyboardType = _mButtons.KeyboardType switch
                {
                    EKeyboardType.ReplyKeyboard => EKeyboardType.InlineKeyBoard,
                    EKeyboardType.InlineKeyBoard => EKeyboardType.ReplyKeyboard,
                    _ => _mButtons.KeyboardType
                };

                break;

            default:
                await Device.Send($"Button clicked with Text: {e.Button.Text} and Value {e.Button.Value}");
                break;
        }
    }
}