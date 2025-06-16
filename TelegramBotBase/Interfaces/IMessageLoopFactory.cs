using System;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Args;
using TelegramBotBase.Base;

namespace TelegramBotBase.Interfaces;

public interface IMessageLoopFactory
{
    Task MessageLoop(BotBase bot, IDeviceSession session, UpdateResult ur, MessageResult e);


    /// <summary>
    /// Configures and returns the set of update types to be used in the application.
    /// </summary>
    /// <returns>An array of <see cref="UpdateType"/> values representing the configured update types.  The array may be empty if
    /// no update types are configured.</returns>
    UpdateType[] ConfigureUpdateTypes();

}