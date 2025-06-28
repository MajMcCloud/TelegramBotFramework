using System;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Args;
using TelegramBotBase.Base;

namespace TelegramBotBase.Interfaces;

public interface IMessageLoopFactory
{
    /// <summary>
    /// Processes incoming messages and updates within the bot's message loop.
    /// </summary>
    /// <param name="bot">The bot instance responsible for handling the message loop.</param>
    /// <param name="session">The device session associated with the current user or client.</param>
    /// <param name="ur">The update result containing information about the latest updates or events.</param>
    /// <param name="e">The message result representing the incoming message or event to be processed.</param>
    /// <returns>A task that represents the asynchronous operation of the message loop.</returns>
    Task MessageLoop(BotBase bot, IDeviceSession session, UpdateResult ur, MessageResult e);


    /// <summary>
    /// Configures and returns the set of update types to be used in the application.
    /// </summary>
    /// <returns>An array of <see cref="UpdateType"/> values representing the configured update types.  The array may be empty if
    /// no update types are configured.</returns>
    UpdateType[] ConfigureUpdateTypes();

}