using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Controls.Inline;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests.Controls;

public class CalendarPickerForm : AutoCleanForm
{
    public CalendarPickerForm()
    {
        DeleteMode = EDeleteMode.OnLeavingForm;
        Init += CalendarPickerForm_Init;
    }

    public CalendarPicker Picker { get; set; }

    private int? SelectedDateMessage { get; set; }

    private Task CalendarPickerForm_Init(object sender, InitEventArgs e)
    {
        Picker = new CalendarPicker
        {
            Title = "Datum auswählen / Pick date"
        };

        AddControl(Picker);
        return Task.CompletedTask;
    }


    public override async Task Action(MessageResult message)
    {
        switch (message.RawData)
        {
            case "back":

                var s = new Menu();

                await NavigateTo(s);

                break;
        }
    }

    public override async Task Render(MessageResult message)
    {
        var s = "";

        s = "Selected date is " + Picker.SelectedDate.ToShortDateString() + "\r\n";
        s += "Selected month is " + Picker.Culture.DateTimeFormat.MonthNames[Picker.VisibleMonth.Month - 1] + "\r\n";
        s += "Selected year is " + Picker.VisibleMonth.Year;

        var bf = new ButtonForm();
        bf.AddButtonRow(new ButtonBase("Back", "back"));

        if (SelectedDateMessage != null)
        {
            await Device.Edit(SelectedDateMessage.Value, s, bf);
        }
        else
        {
            var m = await Device.Send(s, bf);
            SelectedDateMessage = m.MessageId;
        }
    }
}