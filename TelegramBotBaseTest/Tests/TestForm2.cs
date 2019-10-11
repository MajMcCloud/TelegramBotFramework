using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBaseTest.Tests
{
    public class TestForm2 : FormBase
    {


        public TestForm2()
        {
            this.Opened += TestForm2_Opened;
            this.Closed += TestForm2_Closed;
        }

        private async Task TestForm2_Opened(object sender, EventArgs e)
        {
            await this.Device.Send("Welcome to Form 2");
        }

        private async Task TestForm2_Closed(object sender, EventArgs e)
        {
            await this.Device.Send("Ciao from Form 2");
        }




        public override async Task Action(MessageResult message)
        {
            var call = message.GetData<CallbackData>();

            await message.ConfirmAction();

            await message.DeleteMessage();

            message.Handled = true;

            if (call.Value == "testform1")
            {

                var tf = new TestForm();

                await this.NavigateTo(tf);
            }
            else if (call.Value == "alert")
            {
                AlertDialog ad = new AlertDialog("This is a message", "Ok");

                ad.ButtonClicked += async (s, en) =>
                {
                    var fto = new TestForm2();
                    await this.NavigateTo(fto);
                };

                await this.NavigateTo(ad);
            }
            else if (call.Value == "confirm")
            {
                ConfirmDialog pd = new ConfirmDialog("Please confirm", new ButtonBase("Ok", "ok"), new ButtonBase("Cancel", "cancel"));

                pd.ButtonClicked += async (s, en) =>
                {
                    var tf = new TestForm2();

                    await pd.NavigateTo(tf);
                };

                await this.NavigateTo(pd);
            }
            else if (call.Value == "prompt")
            {
                PromptDialog pd = new PromptDialog("Please tell me your name ?");

                pd.Completed += async (s, en) =>
                {
                    await this.Device.Send("Hello " + pd.Value);

                    var tf = new TestForm2();
                    await pd.NavigateTo(tf);
                };

                await this.NavigateTo(pd);
            }


        }

        public override async Task Render(MessageResult message)
        {

            Bitmap bmp = new Bitmap(800, 600);
            using (Graphics g = Graphics.FromImage(bmp))
            {

                g.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height);

                g.DrawString("Test Image", new Font("Arial", 24, FontStyle.Bold, GraphicsUnit.Pixel), Brushes.Black, new PointF(50, 50));

            }

            await this.Device.SetAction(Telegram.Bot.Types.Enums.ChatAction.UploadPhoto);

            ButtonForm btn = new ButtonForm();

            //btn.AddButtonRow(new ButtonBase("Zum Testformular 1", CallbackData.Create("navigate", "testform1")), new ButtonBase("Zum Testformular 1", CallbackData.Create("navigate", "testform1")));

            btn.AddButtonRow(new ButtonBase("Information Prompt", CallbackData.Create("navigate", "alert")));

            btn.AddButtonRow(new ButtonBase("Confirmation Prompt with event", CallbackData.Create("navigate", "confirm")));

            btn.AddButtonRow(new ButtonBase("Request Prompt", CallbackData.Create("navigate", "prompt")));


            await this.Device.SendPhoto(bmp, "Test", btn);

        }


    }
}
