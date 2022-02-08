using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Interfaces
{
    public interface IMessageLoopFactory
    {

        Task MessageLoop(BotBase Bot, DeviceSession session, UpdateResult ur, MessageResult e);

        event EventHandler<UnhandledCallEventArgs> UnhandledCall;


    }
}
