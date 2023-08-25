using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests.Controls;

public class ButtonGridPagingForm : AutoCleanForm
{
    private ButtonGrid _mButtons;

    public ButtonGridPagingForm()
    {
        DeleteMode = EDeleteMode.OnLeavingForm;

        Init += ButtonGridForm_Init;
    }

    private Task ButtonGridForm_Init(object sender, InitEventArgs e)
    {
        _mButtons = new ButtonGrid
        {
            KeyboardType = EKeyboardType.ReplyKeyboard,
            EnablePaging = true,
            EnableSearch = true,
            HeadLayoutButtonRow = new List<ButtonBase> { new("Back", "back") }
        };

        var countries = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

        var bf = new ButtonForm();

        foreach (var c in countries)
        {
            bf.AddButtonRow(new ButtonBase(c.EnglishName, c.EnglishName));
        }

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
        else
        {
            await Device.Send($"Button clicked with Text: {e.Button.Text} and Value {e.Button.Value}");
        }
    }
}
