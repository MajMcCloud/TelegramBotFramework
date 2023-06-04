using System;
using System.Drawing;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Base;
using TelegramBotBase.Extensions.Images;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests;

public class TestForm2 : FormBase
{
    public TestForm2()
    {
        Opened += TestForm2_Opened;
        Closed += TestForm2_Closed;
    }

    private async Task TestForm2_Opened(object sender, EventArgs e)
    {
        await Device.Send("Welcome to Form 2");
    }

    private async Task TestForm2_Closed(object sender, EventArgs e)
    {
        await Device.Send("Ciao from Form 2");
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

            await NavigateTo(tf);
        }
        else if (call.Value == "alert")
        {
            var ad = new AlertDialog("This is a message", "Ok");

            ad.ButtonClicked += async (s, en) =>
            {
                var fto = new TestForm2();
                await NavigateTo(fto);
            };

            await OpenModal(ad);
        }
        else if (call.Value == "confirm")
        {
            var pd = new ConfirmDialog("Please confirm", new ButtonBase("Ok", "ok"),
                                       new ButtonBase("Cancel", "cancel"));

            pd.ButtonClicked += async (s, en) =>
            {
                var tf = new TestForm2();

                await pd.NavigateTo(tf);
            };

            await OpenModal(pd);
        }
        else if (call.Value == "prompt")
        {
            var pd = new PromptDialog("Please tell me your name ?");

            pd.Completed += async (s, en) => { await Device.Send("Hello " + pd.Value); };

            await OpenModal(pd);
        }
    }

    public override async Task Render(MessageResult message)
    {
        var bmp = new Bitmap(800, 600);
        using (var g = Graphics.FromImage(bmp))
        {
            g.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height);

            g.DrawString("Test Image", new Font("Arial", 24, FontStyle.Bold, GraphicsUnit.Pixel), Brushes.Black,
                         new PointF(50, 50));
        }

        await Device.SetAction(ChatAction.UploadPhoto);

        var btn = new ButtonForm();

        //btn.AddButtonRow(new ButtonBase("Zum Testformular 1", CallbackData.Create("navigate", "testform1")), new ButtonBase("Zum Testformular 1", CallbackData.Create("navigate", "testform1")));

        btn.AddButtonRow(new ButtonBase("Information Prompt", CallbackData.Create("navigate", "alert")));

        btn.AddButtonRow(new ButtonBase("Confirmation Prompt with event", CallbackData.Create("navigate", "confirm")));

        btn.AddButtonRow(new ButtonBase("Request Prompt", CallbackData.Create("navigate", "prompt")));


        await Device.SendPhoto(bmp, "Test", "", btn);
    }
}