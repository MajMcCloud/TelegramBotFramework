using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Base;

namespace TelegramBotBase.Form
{
    public class GroupForm : FormBase
    {


        public GroupForm()
        {


        }

        public override async Task Load(MessageResult message)
        {
            switch (message.MessageType)
            {
                case Telegram.Bot.Types.Enums.MessageType.ChatMembersAdded:

                    await OnMemberChanges(new MemberChangeEventArgs(Telegram.Bot.Types.Enums.MessageType.ChatMembersAdded, message, message.Message.NewChatMembers));

                    break;
                case Telegram.Bot.Types.Enums.MessageType.ChatMemberLeft:

                    await OnMemberChanges(new MemberChangeEventArgs(Telegram.Bot.Types.Enums.MessageType.ChatMemberLeft, message, message.Message.LeftChatMember));

                    break;

                case Telegram.Bot.Types.Enums.MessageType.ChatPhotoChanged:
                case Telegram.Bot.Types.Enums.MessageType.ChatPhotoDeleted:
                case Telegram.Bot.Types.Enums.MessageType.ChatTitleChanged:
                case Telegram.Bot.Types.Enums.MessageType.MigratedFromGroup:
                case Telegram.Bot.Types.Enums.MessageType.MigratedToSupergroup:
                case Telegram.Bot.Types.Enums.MessageType.MessagePinned:
                case Telegram.Bot.Types.Enums.MessageType.GroupCreated:
                case Telegram.Bot.Types.Enums.MessageType.SupergroupCreated:
                case Telegram.Bot.Types.Enums.MessageType.ChannelCreated:

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

        public virtual async Task OnMemberChanges(MemberChangeEventArgs e)
        {

        }


        public virtual async Task OnGroupChanged(GroupChangedEventArgs e)
        {

        }


        public virtual async Task OnMessage(MessageResult e)
        {

        }

        public virtual async Task OnMessageEdit(MessageResult e)
        {

        }
    }
}
