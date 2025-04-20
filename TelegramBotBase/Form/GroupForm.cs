using System;
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
            case MessageType.NewChatMembers:

                await OnMemberChanges(new MemberChangeEventArgs(MessageType.NewChatMembers, message,
                                                                message.Message.NewChatMembers));

                break;
            case MessageType.LeftChatMember:

                await OnMemberChanges(new MemberChangeEventArgs(MessageType.LeftChatMember, message,
                                                                message.Message.LeftChatMember));

                break;

            case MessageType.NewChatPhoto:
            case MessageType.DeleteChatPhoto:
            case MessageType.NewChatTitle:
            case MessageType.MigrateFromChatId:
            case MessageType.MigrateToChatId:
            case MessageType.PinnedMessage:
            case MessageType.GroupChatCreated:
            case MessageType.SupergroupChatCreated:
            case MessageType.ChannelChatCreated:

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