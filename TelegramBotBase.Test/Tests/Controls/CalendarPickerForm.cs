using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Controls;
using TelegramBotBase.Args;
using TelegramBotBase.Controls.Inline;

namespace TelegramBotBaseTest.Tests.Controls
{
    public class CalendarPickerForm : AutoCleanForm
    {

        public CalendarPicker Picker { get; set; }

        int? selectedDateMessage { get; set; }

        public CalendarPickerForm()
        {
            this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;
            this.Init += CalendarPickerForm_Init;
        }

        private async Task CalendarPickerForm_Init(object sender, InitEventArgs e)
        {
            this.Picker = new CalendarPicker();
            this.Picker.Title = "Datum auswählen / Pick date";
            
            this.AddControl(Picker);
        }


        public override async Task Action(MessageResult message)
        {

            switch(message.RawData)
            {
                case "back":

                    var s = new Menu();

                    await this.NavigateTo(s);

                    break;
            }

        }

        public override async Task Render(MessageResult message)
        {
            String s = "";

            s = "Selected date is " + this.Picker.SelectedDate.ToShortDateString() + "\r\n";
            s += "Selected month is " + this.Picker.Culture.DateTimeFormat.MonthNames[this.Picker.VisibleMonth.Month - 1] + "\r\n";
            s += "Selected year is " + this.Picker.VisibleMonth.Year.ToString();

            ButtonForm bf = new ButtonForm();
            bf.AddButtonRow(new ButtonBase("Back","back"));

            if (selectedDateMessage != null)
            {
                await this.Device.Edit(this.selectedDateMessage.Value, s, bf);
            }
            else
            {
                var m = await this.Device.Send(s, bf);
                this.selectedDateMessage = m.MessageId;
            }



        }



    }
}
