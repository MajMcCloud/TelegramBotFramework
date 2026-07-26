using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Controls.Hybrid;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;
using static System.Net.Mime.MediaTypeNames;

namespace ModalDialogNavigation.Forms.Navigation
{
    public class Version4 : AutoCleanForm
    {
        private ButtonGrid _mButtons;

        public Version4()
        {
            DeleteMode = EDeleteMode.OnLeavingForm;

            Init += ButtonGridForm_Init;
        }

        private Task ButtonGridForm_Init(object sender, InitEventArgs e)
        {
            _mButtons = new ButtonGrid
            {
                KeyboardType = EKeyboardType.InlineKeyBoard
            };

            var bf = new ButtonForm();

            bf.AddButtonRow(new ButtonBase("Back", "back"), new ButtonBase("Switch Keyboard", "switch"));

            bf.AddButtonRow(new ButtonBase("Open Prompt", "prompt"));

            _mButtons.DataSource.ButtonForm = bf;

            _mButtons.ButtonClicked += Bg_ButtonClicked;


            //Add a title to about version 4 and this should not be allowed to call NavigateTo cause it has been opened as a modal dialog
            _mButtons.Title = "Version 4 - ButtonGrid, Modal Prompt Dialog + illegal NavigateTo";
            _mButtons.Title += "\r\n\r\n'Open Prompt' opens a PromptDialog as a MODAL dialog (OpenModal). Its Completed handler then tries to call NavigateTo from the modal context.";
            _mButtons.Title += "\r\n\r\nExpected: an InvalidOperationException is thrown on purpose. You must not navigate away from a modal dialog context, only from the parent form. The bot reports whether the exception was raised correctly. You can also switch the keyboard type - try it out!";

            AddControl(_mButtons);
            return Task.CompletedTask;
        }

        public override Task Load(MessageResult message)
        {
            return base.Load(message);
        }

        private async Task Bg_ButtonClicked(object sender, ButtonClickedEventArgs e)
        {
            if (e.Button == null)
            {
                return;
            }

            if (e.Button.Value == "back")
            {
                var start = new Start();
                await NavigateTo(start);
            }
            else if (e.Button.Value == "switch")
            {
                _mButtons.KeyboardType = _mButtons.KeyboardType switch
                {
                    EKeyboardType.ReplyKeyboard => EKeyboardType.InlineKeyBoard,
                    EKeyboardType.InlineKeyBoard => EKeyboardType.ReplyKeyboard,
                    _ => _mButtons.KeyboardType
                };
            }
            else if (e.Button.Value == "prompt")
            {
                var pd = new PromptDialog("Please tell me your name ?");

                pd.Completed += Pd_Completed;

                await OpenModal(pd);
            }
        }

        private async Task Pd_Completed(object sender, PromptDialogCompletedEventArgs e)
        {
            var pd = sender as PromptDialog;

            await Device.Send("Hello " + e.Value); 
            
            var v4 = new Version4();

            try
            {
                await pd.NavigateTo(v4);
            }
            catch (InvalidOperationException)
            {
                //This will throw an exception because we are trying to navigate to another form from a modal dialog, which is not allowed. So exception has been thrown successfully.
                await Device.Send("Exception thrown. You can not navigate to another form from a modal dialog, which is not allowed.");
                return;
            }
            
            //No exception has been thrown, invalid behaviour, let the user know
            await Device.Send("No exception thrown. This is NOT the expected behaviour, you should not be able to navigate to another form from a modal dialog.");
        }
    }
}
