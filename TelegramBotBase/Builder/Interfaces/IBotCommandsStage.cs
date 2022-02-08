using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types;

namespace TelegramBotBase.Builder.Interfaces
{
    public interface IBotCommandsStage
    {
        /// <summary>
        /// Does not create any commands.
        /// </summary>
        /// <returns></returns>
        ISessionSerializationStage NoCommands();


        /// <summary>
        /// Creates default commands for start, help and settings.
        /// </summary>
        /// <returns></returns>
        ISessionSerializationStage DefaultCommands();


        /// <summary>
        /// Only adds the start command.
        /// </summary>
        /// <returns></returns>
        ISessionSerializationStage OnlyStart();


        /// <summary>
        /// Gives you the ability to add custom commands.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>

        ISessionSerializationStage CustomCommands(Action<Dictionary<BotCommandScope, List<BotCommand>>> action);


    }
}
