using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Args;
using TelegramBotBase.Base;

namespace TelegramBotBase.Form;

public class GroupForm : FormBase
{
    public override async Task Load(MessageResult message)
    {
        switch (message.MessageType)
        {
            case MessageType.ChatMembersAdded:

                await OnMemberChanges(new MemberChangeEventArgs(MessageType.ChatMembersAdded, message,
                                                                message.Message.NewChatMembers));

                break;
            case MessageType.ChatMemberLeft:

                await OnMemberChanges(new MemberChangeEventArgs(MessageType.ChatMemberLeft, message,
                                                                message.Message.LeftChatMember));

                break;

            case MessageType.ChatPhotoChanged:
            case MessageType.ChatPhotoDeleted:
            case MessageType.ChatTitleChanged:
            case MessageType.MigratedFromGroup:
            case MessageType.MigratedToSupergroup:
            case MessageType.MessagePinned:
            case MessageType.GroupCreated:
            case MessageType.SupergroupCreated:
            case MessageType.ChannelCreated:

                await OnGroupChanged(new GroupChangedEventArgs(message.MessageType, message));

                break;

            default:

                await OnMessage(message);

                break;
        }
    }

    public override async Task Edited(MessageResult message)
    {
        await OnMessageEdit(message);
    }

    public virtual Task OnMemberChanges(MemberChangeEventArgs e)
    {
        return Task.CompletedTask;
    }


    public virtual Task OnGroupChanged(GroupChangedEventArgs e)
    {
        return Task.CompletedTask;
    }


    public virtual Task OnMessage(MessageResult e)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnMessageEdit(MessageResult e)
    {
        return Task.CompletedTask;
    }
}