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
    public class Version2 : AutoCleanForm
    {
        private ButtonGrid _mButtons;

        public Version2()
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

                pd.Completed += async (s, en) => { await Device.Send("Hello " + pd.Value); };

                await OpenModal(pd);
            }
        }



    }
}
