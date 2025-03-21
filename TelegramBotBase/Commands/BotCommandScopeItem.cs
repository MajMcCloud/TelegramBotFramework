using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBotBase.Base;

namespace TelegramBotBase.Commands
{
    /// <summary>
    /// Contains a list of BotCommands for a specific scope with additional information.
    /// </summary>
    [DebuggerDisplay("Scope: {Scope.Type}, Language: {(Language ?? \"default\")}, Commands: {Commands.Count}")]
    public class BotCommandScopeGroup
    {
        public BotCommandScope Scope { get; set; }

        public List<BotCommand> Commands { get; set; }

        public String Language { get; set; }

        /// <summary>
        /// If set to true, this scope will be removed from the bot.
        /// </summary>
        public bool Remove { get; set; } = false;

        public BotCommandScopeGroup(BotCommandScope botCommandScope, List<BotCommand> commands)
        {
            Scope = botCommandScope;
            Commands = commands;
        }

        public BotCommandScopeGroup(BotCommandScope botCommandScope, List<BotCommand> commands, string language)
        {
            Scope = botCommandScope;
            Commands = commands;
            Language = language;
        }

        public BotCommandScopeGroup(BotCommandScope botCommandScope, BotCommand[] commands, string language)
        {
            Scope = botCommandScope;
            Commands = commands.ToList();
            Language = language;
        }

        /// <summary>
        /// Checks if this scope has a specific command.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public bool HasCommand(String command)
        {
            foreach (var c in this.Commands)
            {
                if (Constants.Telegram.BotCommandIndicator + c.Command == command)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Deletes all commands from this scope and language.
        /// </summary>
        /// <param name="client"></param>
        public async Task ResetCommand(MessageClient client)
        {
            await client.DeleteBotCommands(Scope, Language);
        }

    }
}
