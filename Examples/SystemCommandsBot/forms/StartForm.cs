using System;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace SystemCommandsBot.forms
{
    public class StartForm : FormBase
    {
        public string Password { get; set; }

        public override Task Load(MessageResult message)
        {
            var inp = message.MessageText;
            if (Program.BotConfig.Password == inp)
            {
                Password = inp;
            }

            return Task.CompletedTask;
        }


        public override async Task Render(MessageResult message)
        {
            if (Password == null || Password.Trim() == "")
            {
                await Device.Send("Bitte gib dein Passwort an.");
                return;
            }


            var cmd = new CmdForm();
            cmd.ExpiresAt = DateTime.Now.AddDays(14);

            await NavigateTo(cmd);

        }

    }
}
