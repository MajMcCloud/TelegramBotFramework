using System;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Base;

namespace TelegramBotBase.Interfaces;

public interface IMessageLoopFactory
{
    Task MessageLoop(BotBase bot, IDeviceSession session, UpdateResult ur, MessageResult e);

}