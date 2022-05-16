using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Telegram.Bot.Types;

namespace TelegramBotBase.Commands
{
    public static class Extensions
    {
        /// <summary>
        /// Adding the command with a description.
        /// </summary>
        /// <param name="cmds"></param>
        /// <param name="command"></param>
        /// <param name="description"></param>
        public static void Add(this Dictionary<BotCommandScope, List<BotCommand>> cmds, String command, String description, BotCommandScope scope = null)
        {
            if (scope == null)
            {
                scope = BotCommandScope.Default();
            }

            var item = cmds.FirstOrDefault(a => a.Key.Type == scope.Type);

            if (item.Value != null)
            {
                item.Value.Add(new BotCommand() { Command = command, Description = description });
            }
            else
            {
                cmds.Add(scope, new List<BotCommand> { new BotCommand() { Command = command, Description = description } });
            }
        }

        /// <summary>
        /// Adding the command with a description.
        /// </summary>
        /// <param name="cmds"></param>
        /// <param name="command"></param>
        /// <param name="description"></param>
        public static void Clear(this Dictionary<BotCommandScope, List<BotCommand>> cmds, BotCommandScope scope = null)
        {
            if (scope == null)
            {
                scope = BotCommandScope.Default();
            }

            var item = cmds.FirstOrDefault(a => a.Key.Type == scope.Type);

            if (item.Key != null)
            {
                cmds[item.Key] = null;
            }
            else
            {
                cmds[scope] = null;
            }
        }

        /// <summary>
        /// Adding the default /start command with a description.
        /// </summary>
        /// <param name="cmds"></param>
        /// <param name="description"></param>
        public static void Start(this Dictionary<BotCommandScope, List<BotCommand>> cmds, String description) => Add(cmds, "start", description, null);

        /// <summary>
        /// Adding the default /help command with a description.
        /// </summary>
        /// <param name="cmds"></param>
        /// <param name="description"></param>
        public static void Help(this Dictionary<BotCommandScope, List<BotCommand>> cmds, String description) => Add(cmds, "help", description, null);

        /// <summary>
        /// Adding the default /settings command with a description.
        /// </summary>
        /// <param name="cmds"></param>
        /// <param name="description"></param>
        public static void Settings(this Dictionary<BotCommandScope, List<BotCommand>> cmds, String description) => Add(cmds, "settings", description, null);

        /// <summary>
        /// Clears all default commands.
        /// </summary>
        /// <param name="cmds"></param>
        public static void ClearDefaultCommands(this Dictionary<BotCommandScope, List<BotCommand>> cmds) => Clear(cmds, null);

        /// <summary>
        /// Clears all commands of a specific device.
        /// </summary>
        /// <param name="cmds"></param>
        public static void ClearChatCommands(this Dictionary<BotCommandScope, List<BotCommand>> cmds, long DeviceId) => Clear(cmds, new BotCommandScopeChat() { ChatId = DeviceId });

        /// <summary>
        /// Adding a chat command with a description.
        /// </summary>
        /// <param name="cmds"></param>
        /// <param name="command"></param>
        /// <param name="description"></param>
        public static void AddChatCommand(this Dictionary<BotCommandScope, List<BotCommand>> cmds, long DeviceId, String command, String description) => Add(cmds, command, description, new BotCommandScopeChat() { ChatId = DeviceId });

        /// <summary>
        /// Adding a group command with a description.
        /// </summary>
        /// <param name="cmds"></param>
        /// <param name="command"></param>
        /// <param name="description"></param>
        public static void AddGroupCommand(this Dictionary<BotCommandScope, List<BotCommand>> cmds, String command, String description) => Add(cmds, command, description, new BotCommandScopeAllGroupChats());

        /// <summary>
        /// Clears all group commands.
        /// </summary>
        /// <param name="cmds"></param>
        public static void ClearGroupCommands(this Dictionary<BotCommandScope, List<BotCommand>> cmds) => Clear(cmds, new BotCommandScopeAllGroupChats());

        /// <summary>
        /// Adding group admin command with a description.
        /// </summary>
        /// <param name="cmds"></param>
        /// <param name="command"></param>
        /// <param name="description"></param>
        public static void AddGroupAdminCommand(this Dictionary<BotCommandScope, List<BotCommand>> cmds, String command, String description) => Add(cmds, command, description, new BotCommandScopeAllChatAdministrators());

        /// <summary>
        /// Clears all group admin commands.
        /// </summary>
        /// <param name="cmds"></param>
        public static void ClearGroupAdminCommand(this Dictionary<BotCommandScope, List<BotCommand>> cmds) => Clear(cmds, new BotCommandScopeAllChatAdministrators());

        /// <summary>
        /// Adding a privat command with a description.
        /// </summary>
        /// <param name="cmds"></param>
        /// <param name="command"></param>
        /// <param name="description"></param>
        public static void AddPrivateChatCommand(this Dictionary<BotCommandScope, List<BotCommand>> cmds, String command, String description) => Add(cmds, command, description, new BotCommandScopeAllPrivateChats());

        /// <summary>
        /// Clears all private commands.
        /// </summary>
        /// <param name="cmds"></param>
        public static void ClearPrivateChatCommand(this Dictionary<BotCommandScope, List<BotCommand>> cmds) => Clear(cmds, new BotCommandScopeAllPrivateChats());
    }
}
