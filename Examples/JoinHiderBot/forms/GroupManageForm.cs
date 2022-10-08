using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Args;
using TelegramBotBase.Form;

namespace JoinHiderBot.forms;

public class GroupManageForm : GroupForm
{
    public override async Task OnMemberChanges(MemberChangeEventArgs e)
    {
        if (e.Type != MessageType.ChatMembersAdded && e.Type != MessageType.ChatMemberLeft)
        {
            return;
        }


        var m = e.Result.Message;

        await Device.DeleteMessage(m);
    }
}