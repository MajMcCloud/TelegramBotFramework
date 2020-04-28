using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Form;

namespace JoinHiderBot.forms
{
    public class GroupManageForm : GroupForm
    {

        public override async Task OnMemberChanges(MemberChangeEventArgs e)
        {
            if (e.Type != Telegram.Bot.Types.Enums.MessageType.ChatMembersAdded && e.Type != Telegram.Bot.Types.Enums.MessageType.ChatMemberLeft)
                return;


            var m = e.Result.Message;

            await this.Device.DeleteMessage(m);
        }

    }
}
