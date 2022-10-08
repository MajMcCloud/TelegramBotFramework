using System;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Interfaces;

public interface IMessageLoopFactory
{
    Task MessageLoop(BotBase bot, DeviceSession session, UpdateResult ur, MessageResult e);

    event EventHandler<UnhandledCallEventArgs> UnhandledCall;
}