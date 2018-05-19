using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Tests
{
    public class TestForm2 : FormBase
    {

        public override async Task Init(params object[] param)
        {

        }

        public override async Task Opened()
        {
            await this.Device.Send("Willkommen in Formular 2");
        }

        public override async Task Closed()
        {
            await this.Device.Send("Tschüss in Formular 2");
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

                await tf.Init();

                await this.NavigateTo(tf);
            }
            else if (call.Value == "alert")
            {
                var fto = new TestForm2();
                await fto.Init();

                AlertDialog ad = new AlertDialog("Dies ist eine Meldung !", "Ok", fto);


                await this.NavigateTo(ad);
            }
            else if (call.Value == "prompt")
            {
                PromptDialog pd = new PromptDialog("Bitte bestätigen !", new ButtonBase("Ok", "ok"), new ButtonBase("Abbrechen", "cancel"));

                var tf = new TestForm2();

                await tf.Init();

                pd.ButtonForms.Add("ok", tf);
                pd.ButtonForms.Add("cancel", tf);

                await this.NavigateTo(pd);
            }


        }

        public override async Task Render(MessageResult message)
        {

            Bitmap bmp = new Bitmap(800, 600);
            using (Graphics g = Graphics.FromImage(bmp))
            {

                g.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height);

                g.DrawString("Testbild", new Font("Arial", 24, FontStyle.Bold, GraphicsUnit.Pixel), Brushes.Black, new PointF(50, 50));

            }

            await this.Device.SetAction(Telegram.Bot.Types.Enums.ChatAction.UploadPhoto);

            ButtonForm btn = new ButtonForm();

            btn.AddButtonRow(new ButtonBase("Zum Testformular 1", CallbackData.Create("navigate", "testform1")), new ButtonBase("Zum Testformular 1", CallbackData.Create("navigate", "testform1")));

            btn.AddButtonRow(new ButtonBase("Info Dialog", CallbackData.Create("navigate", "alert")), new ButtonBase("Bestätigungs Dialog", CallbackData.Create("navigate", "prompt")));


            await this.Device.SendPhoto(bmp, "Test", btn);

        }


    }
}
