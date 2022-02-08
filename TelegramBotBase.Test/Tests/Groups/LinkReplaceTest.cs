using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBaseTest.Tests.Groups
{
    public class LinkReplaceTest : TelegramBotBase.Form.GroupForm
    {

        Dictionary<long, int> Counter { get; set; } = new Dictionary<long, int>();

        private const int Maximum = 3;


        public LinkReplaceTest()
        {
            this.Opened += LinkReplaceTest_Opened;
        }

        private async Task LinkReplaceTest_Opened(object sender, EventArgs e)
        {

            ButtonForm bf = new ButtonForm();

            bf.AddButtonRow(new ButtonBase("Open GroupChange Test", "groupchange"));
            bf.AddButtonRow(new ButtonBase("Open WelcomeUser Test", "welcomeuser"));
            bf.AddButtonRow(new ButtonBase("Open LinkReplace Test", "linkreplace"));

            await this.Device.Send("LinkReplaceTest started, click to switch", bf);

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

        public override async Task OnMessage(MessageResult e)
        {
            var from = e.Message.From.Id;

            if (e.Message.From.IsBot)
                return;

            //Are urls inside his message ?
            if (!HasLinks(e.MessageText))
                return;

            var u = await Device.GetChatUser(from);

            //Don't kick Admins or Creators
            if (u.Status == Telegram.Bot.Types.Enums.ChatMemberStatus.Administrator | u.Status == Telegram.Bot.Types.Enums.ChatMemberStatus.Creator)
            {
                await this.Device.Send("You won't get kicked,...not this time.");
                return;
            }

            await e.Device.DeleteMessage(e.MessageId);

            var cp = new ChatPermissions();
            cp.CanAddWebPagePreviews = false;
            cp.CanChangeInfo = false;
            cp.CanInviteUsers = false;
            cp.CanPinMessages = false;
            cp.CanSendMediaMessages = false;
            cp.CanSendMessages = false;
            cp.CanSendOtherMessages = false;
            cp.CanSendPolls = false;

            //Collect user "mistakes" with sending url, after 3 he gets kicked out.
            if (Counter.ContainsKey(from))
            {
                Counter[from]++;
            }
            else
            {
                Counter[from] = 1;
            }


            if (Counter[from] >= 3)
            {
                await e.Device.KickUser(from);

                await e.Device.Send(e.Message.From.FirstName + " " + e.Message.From.LastName + " has been removed from the group");
            }
            else
            {
                await e.Device.RestrictUser(from, cp, DateTime.UtcNow.AddSeconds(30));

                await e.Device.Send(e.Message.From.FirstName + " " + e.Message.From.LastName + " has been blocked for 30 seconds");
            }



        }

        /// <summary>
        /// Call onmessage also on edited message, if someone want to spoof a failed message and insert a link.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override async Task OnMessageEdit(MessageResult message)
        {
            await OnMessage(message);
        }

        /// <summary>
        /// https://stackoverflow.com/a/20651284
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private bool HasLinks(String str)
        {
            var tmp = str;

            var pattern = @"^(http|https|ftp|)\://|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
            Regex r = new Regex(pattern);

            var matches = r.Matches(tmp);

            return (matches.Count > 0);
        }


    }
}
