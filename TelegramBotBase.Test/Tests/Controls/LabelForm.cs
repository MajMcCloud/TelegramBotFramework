using System;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Controls.Inline;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests.Controls;

public class LabelForm : AutoCleanForm
{

    TelegramBotBase.Controls.Inline.Label _label;

    ButtonGrid _buttonGrid;

    String[] string_options = new string[] { "My test label", "Here is a different text", "*And this looks completely different.*", "Aha! another one.", "_Gotcha!_" };

    public LabelForm()
    {
        DeleteMode = EDeleteMode.OnLeavingForm;

        Init += LabelForm_Init;
    }

    private Task LabelForm_Init(object sender, InitEventArgs e)
    {
        //The "label" control
        _label = new Label("My test label");

        AddControl(_label);

        //Optional...just for experimentation...
        var bf = new ButtonForm();

        bf.AddButtonRow("Toggle text", "toggle");
        bf.AddButtonRow("Open menu", "menu");

        _buttonGrid = new ButtonGrid(bf);
        _buttonGrid.KeyboardType = EKeyboardType.ReplyKeyboard;
        _buttonGrid.Title = "Choose your options:";
        _buttonGrid.ButtonClicked += _buttonGrid_ButtonClicked;

        AddControl(_buttonGrid);

        return Task.CompletedTask;
    }

    private async Task _buttonGrid_ButtonClicked(object sender, ButtonClickedEventArgs e)
    {
        switch (e.Button.Value ?? "")
        {
            case "menu":

                var mn = new Menu();

                await NavigateTo(mn);

                break;

            case "toggle":


                //Pick random string from array
                var r = new Random((int)DateTime.UtcNow.Ticks);

                String random_string;
                do
                {
                    random_string = string_options[r.Next(0, string_options.Length)];
                    if (random_string == null)
                        continue;

                } while (random_string == _label.Text);


                _label.Text = random_string;

                break;

        }
    }
}