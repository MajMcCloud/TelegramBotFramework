using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;

namespace TelegramBotBase.Commands;

public static class Extensions
{
    /// <summary>
    ///     Adding the command with a description.
    /// </summary>
    /// <param name="cmds"></param>
    /// <param name="command"></param>
    /// <param name="description"></param>
    public static void Add(this List<BotCommandScopeGroup> cmds, string command,
                           string description, BotCommandScope scope = null, string language = null)
    {
        if (scope == null)
        {
            scope = BotCommandScope.Default();
        }

        if (string.IsNullOrEmpty(command))
        {
            throw new ArgumentNullException(nameof(command), $"{nameof(command)} parameter can not be null or empty");
        }

        if (command.StartsWith(Constants.Telegram.BotCommandIndicator))
        {
            throw new ArgumentException($"{nameof(command)} parameter does not have to start with a slash, please remove.", $"{nameof(command)}");
        }

        var item = cmds.FirstOrDefault(a => a.Scope.Type == scope.Type && a.Language == language);


        if (item == null)
        {
            cmds.Add(new(scope, new List<BotCommand> { new() { Command = command, Description = description } }, language) { Remove = false });
            return;
        }

        item.Commands.Add(new BotCommand { Command = command, Description = description });

    }

    /// <summary>
    /// Setting the command to be removed. (Needs to be uploaded to the bot to take effect)
    /// </summary>
    /// <param name="cmds"></param>
    /// <param name="scope"></param>
    /// <param name="language"></param>
    public static void Remove(this List<BotCommandScopeGroup> cmds, BotCommandScope scope = null, string language = null)
    {
        if (scope == null)
        {
            scope = BotCommandScope.Default();
        }
        var item = cmds.FirstOrDefault(a => a.Scope.Type == scope.Type && a.Language == language);

        if (item != null)
        {
            item.Remove = true;
        }
    }

    /// <summary>
    ///     Adding the command with a description.
    /// </summary>
    /// <param name="cmds"></param>
    /// <param name="command"></param>
    /// <param name="description"></param>
    public static void Clear(this List<BotCommandScopeGroup> cmds, BotCommandScope scope = null, string language = null)
    {
        if (scope == null)
        {
            scope = BotCommandScope.Default();
        }

        var item = cmds.FirstOrDefault(a => a.Scope.Type == scope.Type && a.Language == language);

        if (item?.Scope != null)
        {
            item.Commands.Clear();
            return;
        }

        if (item != null)
        {
            cmds.Remove(item);
        }

    }

    /// <summary>
    ///     Adding the default /start command with a description.
    /// </summary>
    /// <param name="cmds"></param>
    /// <param name="description"></param>
    public static void Start(this List<BotCommandScopeGroup> cmds, string description, string language = null)
    {
        Add(cmds, "start", description, language: language);
    }

    /// <summary>
    ///     Adding the default /help command with a description.
    /// </summary>
    /// <param name="cmds"></param>
    /// <param name="description"></param>
    public static void Help(this List<BotCommandScopeGroup> cmds, string description, string language = null)
    {
        Add(cmds, "help", description, language: language);
    }

    /// <summary>
    ///     Adding the default /settings command with a description.
    /// </summary>
    /// <param name="cmds"></param>
    /// <param name="description"></param>
    public static void Settings(this List<BotCommandScopeGroup> cmds, string description, string language = null)
    {
        Add(cmds, "settings", description, language: language);
    }

    /// <summary>
    ///     Clears all default commands.
    /// </summary>
    /// <param name="cmds"></param>
    public static void ClearDefaultCommands(this List<BotCommandScopeGroup> cmds)
    {
        Clear(cmds);
    }

    /// <summary>
    ///     Clears all commands of a specific device.
    /// </summary>
    /// <param name="cmds"></param>
    public static void ClearChatCommands(this List<BotCommandScopeGroup> cmds, long deviceId)
    {
        Clear(cmds, new BotCommandScopeChat { ChatId = deviceId });
    }

    /// <summary>
    ///     Adding a chat command with a description.
    /// </summary>
    /// <param name="cmds"></param>
    /// <param name="command"></param>
    /// <param name="description"></param>
    public static void AddChatCommand(this List<BotCommandScopeGroup> cmds, long deviceId,
                                      string command, string description, string language = null)
    {
        Add(cmds, command, description, new BotCommandScopeChat { ChatId = deviceId }, language);
    }

    /// <summary>
    ///     Adding a group command with a description.
    /// </summary>
    /// <param name="cmds"></param>
    /// <param name="command"></param>
    /// <param name="description"></param>
    public static void AddGroupCommand(this List<BotCommandScopeGroup> cmds, string command,
                                       string description, string language = null)
    {
        Add(cmds, command, description, new BotCommandScopeAllGroupChats(), language);
    }

    /// <summary>
    ///     Clears all group commands.
    /// </summary>
    /// <param name="cmds"></param>
    public static void ClearGroupCommands(this List<BotCommandScopeGroup> cmds, string language = null)
    {
        Clear(cmds, new BotCommandScopeAllGroupChats(), language);
    }

    /// <summary>
    ///     Adding group admin command with a description.
    /// </summary>
    /// <param name="cmds"></param>
    /// <param name="command"></param>
    /// <param name="description"></param>
    public static void AddGroupAdminCommand(this List<BotCommandScopeGroup> cmds, string command,
                                            string description, string language = null)
    {
        Add(cmds, command, description, new BotCommandScopeAllChatAdministrators(), language);
    }

    /// <summary>
    ///     Clears all group admin commands.
    /// </summary>
    /// <param name="cmds"></param>
    public static void ClearGroupAdminCommand(this List<BotCommandScopeGroup> cmds, string language = null)
    {
        Clear(cmds, new BotCommandScopeAllChatAdministrators(), language);
    }

    /// <summary>
    ///     Adding a privat command with a description.
    /// </summary>
    /// <param name="cmds"></param>
    /// <param name="command"></param>
    /// <param name="description"></param>
    public static void AddPrivateChatCommand(this List<BotCommandScopeGroup> cmds,
                                             string command, string description, string language = null)
    {
        Add(cmds, command, description, new BotCommandScopeAllPrivateChats(), language);
    }

    /// <summary>
    ///     Clears all private commands.
    /// </summary>
    /// <param name="cmds"></param>
    public static void ClearPrivateChatCommand(this List<BotCommandScopeGroup> cmds, string language = null)
    {
        Clear(cmds, new BotCommandScopeAllPrivateChats(), language);
    }
}