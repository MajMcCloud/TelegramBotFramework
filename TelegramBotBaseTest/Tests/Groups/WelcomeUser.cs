using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Args;

namespace TelegramBotBaseTest.Tests.Groups
{
    public class WelcomeUser : TelegramBotBase.Form.GroupForm
    {

        public WelcomeUser()
        {
            this.Opened += WelcomeUser_Opened;
        }


        private async Task WelcomeUser_Opened(object sender, EventArgs e)
        {

            ButtonForm bf = new ButtonForm();

            bf.AddButtonRow(new ButtonBase("Open GroupChange Test", "groupchange"));
            bf.AddButtonRow(new ButtonBase("Open WelcomeUser Test", "welcomeuser"));
            bf.AddButtonRow(new ButtonBase("Open LinkReplace Test", "linkreplace"));

            await this.Device.Send("WelcomeUser started, click to switch", bf);

        }

        public override async Task Action(MessageResult message)
        {
            if (message.Handled)
                return;

            var bn = message.RawData;

            await message.ConfirmAction();
            message.Handled = true;

            switch (bn)
            {
                case "groupchange":

                    var gc = new GroupChange();

                    await this.NavigateTo(gc);

                    break;
                case "welcomeuser":

                    var wu = new WelcomeUser();

                    await this.NavigateTo(wu);

                    break;
                case "linkreplace":

                    var lr = new LinkReplaceTest();

                    await this.NavigateTo(lr);

                    break;
            }

        }

        public override async Task OnMemberChanges(MemberChangeEventArgs e)
        {

            if (e.Type == Telegram.Bot.Types.Enums.MessageType.ChatMembersAdded)
            {

                await this.Device.Send("Welcome you new members!\r\n\r\n" + e.Members.Select(a => a.FirstName + " " + a.LastName).Aggregate((a, b) => a + "\r\n" + b));

            }
            else if (e.Type == Telegram.Bot.Types.Enums.MessageType.ChatMemberLeft)
            {
                await this.Device.Send(e.Members.Select(a => a.FirstName + " " + a.LastName).Aggregate((a, b) => a + " and " + b) + " has left the group");

            }

        }




    }
}
