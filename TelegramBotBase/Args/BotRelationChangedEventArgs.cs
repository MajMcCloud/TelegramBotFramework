using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Base;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Args
{
    [DebuggerDisplay("{Editor.DeviceId} => {Status} => {BotId}")]
    public class BotRelationChangedEventArgs : EventArgs
    {

        public IDeviceSession Editor { get; set; }

        public long ChatId => Chat?.Id ?? -1;

        public Chat Chat => UpdateResult.RawData.MyChatMember.Chat;

        public ChatType ChatType => Chat.Type;

        public long BotId => UpdateResult.RawData.MyChatMember.NewChatMember.User?.Id ?? -1;

        public ChatMemberStatus Status => UpdateResult.RawData.MyChatMember.NewChatMember.Status;

        public UpdateResult UpdateResult { get; }

        public MessageResult MessageResult { get; }

        public bool IsGroup => ChatType == ChatType.Group | ChatType == ChatType.Supergroup;

        public BotRelationChangedEventArgs(IDeviceSession editor,UpdateResult ur, MessageResult mr)
        {
            Editor = editor;
            UpdateResult = ur;
            MessageResult = mr;
        }

        


    }
}
