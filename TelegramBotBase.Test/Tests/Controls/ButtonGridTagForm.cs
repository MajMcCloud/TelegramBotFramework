using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.DataSources;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests.Controls;

public class ButtonGridTagForm : AutoCleanForm
{
    private TaggedButtonGrid _mButtons;

    public ButtonGridTagForm()
    {
        DeleteMode = EDeleteMode.OnLeavingForm;

        Init += ButtonGridTagForm_Init;
    }

    private Task ButtonGridTagForm_Init(object sender, InitEventArgs e)
    {
        _mButtons = new TaggedButtonGrid
        {
            KeyboardType = EKeyboardType.ReplyKeyboard,
            EnablePaging = true,
            HeadLayoutButtonRow = new List<ButtonBase> { new("Back", "back") }
        };


        var countries = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

        var bf = new ButtonForm();

        foreach (var c in countries)
        {
            bf.AddButtonRow(new TagButtonBase(c.EnglishName, c.EnglishName, c.Parent.EnglishName));
        }

        _mButtons.Tags = countries.Select(a => a.Parent.EnglishName).Distinct().OrderBy(a => a).ToList();
        _mButtons.SelectedTags = countries.Select(a => a.Parent.EnglishName).Distinct().OrderBy(a => a).ToList();

        _mButtons.EnableCheckAllTools = true;

        _mButtons.DataSource = new ButtonFormDataSource(bf);

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

        switch (e.Button.Value)
        {
            case "back":
                var start = new Menu();
                await NavigateTo(start);
                return;
        }


        await Device.Send($"Button clicked with Text: {e.Button.Text} and Value {e.Button.Value}");
    }
}