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
    public class MonthPickerForm : AutoCleanForm
    {

        public MonthPicker Picker { get; set; }

        int? selectedDateMessage { get; set; }

        public MonthPickerForm()
        {
            this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;
            this.Init += MonthPickerForm_Init;
        }

        private async Task MonthPickerForm_Init(object sender, InitEventArgs e)
        {
            this.Picker = new MonthPicker();
            this.Picker.Title = "Monat auswählen / Pick month";
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

            s += "Selected month is " + this.Picker.Culture.DateTimeFormat.MonthNames[this.Picker.SelectedDate.Month - 1] + "\r\n";
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
