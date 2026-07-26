using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Markdown;
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

                //Add a delay, cause this methods belongs to the context of this form, which means modal messages gets deleted on navigation as well.
                pd.Completed += async (s, en) => { await Device.Send("Hello " + pd.Value); await Task.Delay(3000); };

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


            var text = "Version 1 - No controls, Modal Prompt Dialog".Bold();
            text += "\r\n\r\nThis form uses no ButtonGrid control. Pressing 'Open Prompt' opens a PromptDialog as a MODAL dialog (OpenModal).";
            text += "\r\n\r\nExpected: after entering your name you receive a greeting. As the dialog is modal, its messages are cleaned up together with this form on navigation, so a short delay keeps the greeting visible.";

            await Device.Send(text, btn);
        }


    }
}
