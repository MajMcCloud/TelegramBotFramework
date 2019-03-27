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

        public override async Task Init(params object[] param)
        {

        }

        public override async Task Opened()
        {
            await this.Device.Send("Welcome to Form 2");
        }

        public override async Task Closed()
        {
            await this.Device.Send("Ciao from Form 2");
        }

        public override async Task Load(MessageResult message)
        {

        }

        public override async Task Action(MessageResult message)
        {
            var call = message.GetData<CallbackData>();

            await message.ConfirmAction();

            await message.DeleteMessage();

            if (call.Value == "testform1")
            {

                var tf = new TestForm();

                await this.NavigateTo(tf);
            }
            else if (call.Value == "alert")
            {
                var fto = new TestForm2();

                AlertDialog ad = new AlertDialog("This is a message", "Ok", fto);
                
                await this.NavigateTo(ad);
            }
            else if (call.Value == "prompt")
            {
                PromptDialog pd = new PromptDialog("Please confirm");

                pd.AddButton(new ButtonBase("Ok", "ok"));
                pd.AddButton(new ButtonBase("Cancel", "cancel"));

                var tf = new TestForm2();

                pd.ButtonForms.Add("ok", tf);
                pd.ButtonForms.Add("cancel", tf);
               
                await this.NavigateTo(pd);
            }
            else if (call.Value == "promptevt")
            {
                PromptDialog pd = new PromptDialog("Please confirm", new ButtonBase("Ok", "ok"), new ButtonBase("Cancel", "cancel"));

                pd.ButtonClicked += async (s, en) =>
                {
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

            btn.AddButtonRow(new ButtonBase("Confirmation Prompt without event", CallbackData.Create("navigate", "prompt")));

            btn.AddButtonRow(new ButtonBase("Confirmation Prompt with event", CallbackData.Create("navigate", "promptevt")));


            await this.Device.SendPhoto(bmp, "Test", btn);

        }


    }
}
