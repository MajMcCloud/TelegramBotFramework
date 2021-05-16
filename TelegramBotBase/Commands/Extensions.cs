using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types;

namespace TelegramBotBase.Commands
{
    public static class Extensions
    {
        /// <summary>
        /// Adding the default /start command with a description.
        /// </summary>
        /// <param name="cmds"></param>
        /// <param name="description"></param>
        public static void AddStartCommand(this List<BotCommand> cmds, String description)
        {
            cmds.Add(new BotCommand() { Command = "start", Description = description });
        }

        /// <summary>
        /// Adding the default /help command with a description.
        /// </summary>
        /// <param name="cmds"></param>
        /// <param name="description"></param>
        public static void AddHelpCommand(this List<BotCommand> cmds, String description)
        {
            cmds.Add(new BotCommand() { Command = "help", Description = description });
        }

        /// <summary>
        /// Adding the default /settings command with a description.
        /// </summary>
        /// <param name="cmds"></param>
        /// <param name="description"></param>
        public static void AddSettingsCommand(this List<BotCommand> cmds, String description)
        {
            cmds.Add(new BotCommand() { Command = "settings", Description = description });
        }
    }
}
