using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using static System.Net.Mime.MediaTypeNames;

namespace ModalDialogNavigation.Forms.Navigation
{
    public class Version1 : AutoCleanForm
    {

        public override async Task Action(MessageResult message)
        {
            var call = message.GetData<CallbackData>();

            if (call == null)
                return;

            await message.ConfirmAction();

            //Skip invalid data
            if (call.Method != "navigate")
                return;

            await message.DeleteMessage();

            message.Handled = true;

            if (call.Value == "prompt")
            {
                var pd = new PromptDialog("Please tell me your name ?");

                pd.Completed += async (s, en) => { await Device.Send("Hello " + pd.Value); };

                await OpenModal(pd);
            }
            else if (call.Value == "back")
            {
                var mn = new Start();
                await NavigateTo(mn);
            }
        }

        public override async Task Render(MessageResult message)
        {
            
            var btn = new ButtonForm();

            btn.AddButtonRow(new ButtonBase("Open Prompt", CallbackData.Create("navigate", "prompt")));

            btn.AddButtonRow(new ButtonBase("Back to menu", CallbackData.Create("navigate", "back")));


            await Device.Send("Choose your option", btn);
        }


    }
}
