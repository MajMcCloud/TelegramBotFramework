using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;

namespace SystemCommandsBot.forms
{
    public class StartForm : TelegramBotBase.Form.FormBase
    {
        public String Password { get; set; }

        public override async Task Load(MessageResult message)
        {
            var inp = message.MessageText;
            if (Program.BotConfig.Password == inp)
            {
                this.Password = inp;
            }


        }


        public override async Task Render(MessageResult message)
        {
            if (this.Password == null || this.Password.Trim() == "")
            {
                await this.Device.Send("Bitte gib dein Passwort an.");
                return;
            }


            var cmd = new forms.CmdForm();
            cmd.ExpiresAt = DateTime.Now.AddDays(14);

            await this.NavigateTo(cmd);

        }

    }
}
