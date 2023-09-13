using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Controls.Inline;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests.Controls;

public class MultiToggleButtonForm : AutoCleanForm
{
    public MultiToggleButtonForm()
    {
        DeleteMode = EDeleteMode.OnLeavingForm;

        Init += ToggleButtons_Init;
    }

    private Task ToggleButtons_Init(object sender, InitEventArgs e)
    {
        var mtb = new MultiToggleButton
        {
            Options = new List<ButtonBase> { new("Option 1", "1"), new("Option 2", "2"), new("Option 3", "3") }
        };

        mtb.SelectedOption = mtb.Options.FirstOrDefault();
        mtb.Toggled += Tb_Toggled;
        AddControl(mtb);

        mtb = new MultiToggleButton
        {
            Options = new List<ButtonBase> { new("Option 4", "4"), new("Option 5", "5"), new("Option 6", "6") }
        };

        mtb.SelectedOption = mtb.Options.FirstOrDefault();
        mtb.AllowEmptySelection = false;
        mtb.Toggled += Tb_Toggled;
        AddControl(mtb);
        return Task.CompletedTask;
    }

    private void Tb_Toggled(object sender, EventArgs e)
    {
        var tb = sender as MultiToggleButton;
        if (tb.SelectedOption != null)
        {
            Console.WriteLine(tb.Id + " was pressed, and toggled to " + tb.SelectedOption.Value);
            return;
        }

        Console.WriteLine("Selection for " + tb.Id + " has been removed.");
    }
}