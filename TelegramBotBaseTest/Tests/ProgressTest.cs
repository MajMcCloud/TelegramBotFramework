using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBaseTest.Tests
{
    public class ProgressTest : AutoCleanForm
    {

        public ProgressTest()
        {
            this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;
            this.Opened += ProgressTest_Opened;
            this.Closed += ProgressTest_Closed;
        }


        private async Task ProgressTest_Opened(object sender, EventArgs e)
        {
            await this.Device.Send("Welcome to ProgressTest");
        }

        public override async Task Action(MessageResult message)
        {
            var call = message.GetData<CallbackData>();

            await message.ConfirmAction();


            if (call == null)
                return;

            TelegramBotBase.Controls.ProgressBar Bar = null;

            switch (call.Value)
            {
                case "standard":

                    Bar = new TelegramBotBase.Controls.ProgressBar(0, 100, TelegramBotBase.Controls.ProgressBar.eProgressStyle.standard);
                    Bar.Device = this.Device;

                    break;

                case "squares":

                    Bar = new TelegramBotBase.Controls.ProgressBar(0, 100, TelegramBotBase.Controls.ProgressBar.eProgressStyle.squares);
                    Bar.Device = this.Device;

                    break;

                case "circles":

                    Bar = new TelegramBotBase.Controls.ProgressBar(0, 100, TelegramBotBase.Controls.ProgressBar.eProgressStyle.circles);
                    Bar.Device = this.Device;

                    break;

                case "lines":

                    Bar = new TelegramBotBase.Controls.ProgressBar(0, 100, TelegramBotBase.Controls.ProgressBar.eProgressStyle.lines);
                    Bar.Device = this.Device;

                    break;

                case "squaredlines":

                    Bar = new TelegramBotBase.Controls.ProgressBar(0, 100, TelegramBotBase.Controls.ProgressBar.eProgressStyle.squaredLines);
                    Bar.Device = this.Device;

                    break;

                case "start":

                    var sf = new Menu();

                    await this.NavigateTo(sf);

                    return;

                default:

                    return;

            }


            //Render Progress bar and show some "example" progress
            await Bar.Render(message);

            this.Controls.Add(Bar);

            for (int i = 0; i <= 100; i++)
            {
                Bar.Value++;
                await Bar.Render(message);

                Thread.Sleep(250);
            }


        }


        public override async Task Render(MessageResult message)
        {
            ButtonForm btn = new ButtonForm();
            btn.AddButtonRow(new ButtonBase("Standard", new CallbackData("a", "standard").Serialize()), new ButtonBase("Squares", new CallbackData("a", "squares").Serialize()));

            btn.AddButtonRow(new ButtonBase("Circles", new CallbackData("a", "circles").Serialize()), new ButtonBase("Lines", new CallbackData("a", "lines").Serialize()));

            btn.AddButtonRow(new ButtonBase("Squared Line", new CallbackData("a", "squaredlines").Serialize()));

            btn.AddButtonRow(new ButtonBase("Back to start", new CallbackData("a", "start").Serialize()));

            await this.Device.Send("Choose your progress bar:", btn);
        }

        private async Task ProgressTest_Closed(object sender, EventArgs e)
        {
            await this.Device.Send("Ciao from ProgressTest");
        }

    }
}
