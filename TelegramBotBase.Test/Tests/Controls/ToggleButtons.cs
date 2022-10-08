using System;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Controls.Inline;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace TelegramBotBaseTest.Tests.Controls;

public class ToggleButtons : AutoCleanForm
{
    public ToggleButtons()
    {
        DeleteMode = EDeleteMode.OnLeavingForm;

        Init += ToggleButtons_Init;
    }

    private Task ToggleButtons_Init(object sender, InitEventArgs e)
    {
        var tb = new ToggleButton
        {
            Checked = true
        };
        tb.Toggled += Tb_Toggled;

        AddControl(tb);

        tb = new ToggleButton
        {
            Checked = false
        };
        tb.Toggled += Tb_Toggled;

        AddControl(tb);

        tb = new ToggleButton
        {
            Checked = true
        };
        tb.Toggled += Tb_Toggled;

        AddControl(tb);
        return Task.CompletedTask;
    }

    private void Tb_Toggled(object sender, EventArgs e)
    {
        var tb = sender as ToggleButton;
        Console.WriteLine(tb.Id + " was pressed, and toggled to " + (tb.Checked ? "Checked" : "Unchecked"));
    }
}