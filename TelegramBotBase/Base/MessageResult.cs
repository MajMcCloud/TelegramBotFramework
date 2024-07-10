using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBotBase.Base;

public class MessageResult : ResultBase
{
    public MessageResult(Update update)
    {
        UpdateData = update;

        init();
    }

    void init()
    {
        IsAction = UpdateData.CallbackQuery != null;

        if (Message == null)
            return;

        IsBotCommand = Message.Entities?.Any(a => a.Type == MessageEntityType.BotCommand) ?? false;

        if (!IsBotCommand)
            return;

        BotCommand = MessageText.Split(' ')[0];

        IsBotGroupCommand = BotCommand.Contains("@");

        if (IsBotGroupCommand)
        {
            BotCommand = BotCommand.Substring(0, BotCommand.LastIndexOf('@'));
        }
    }

    public Update UpdateData { get; private set; }

    /// <summary>
    ///     Returns the Device/ChatId
    /// </summary>
    public override long DeviceId =>
        UpdateData?.Message?.Chat?.Id
        ?? UpdateData?.EditedMessage?.Chat.Id
        ?? UpdateData?.CallbackQuery.Message?.Chat.Id
        ?? Device?.DeviceId
        ?? 0;

    /// <summary>
    ///     The message id
    /// </summary>
    public override int MessageId =>
        UpdateData?.Message?.MessageId
        ?? Message?.MessageId
        ?? UpdateData?.CallbackQuery?.Message?.MessageId
        ?? 0;

    public string Command => UpdateData?.Message?.Text ?? "";

    public string MessageText => UpdateData?.Message?.Text ?? "";

    public MessageType MessageType => Message?.Type ?? MessageType.Unknown;

    public override Message Message =>
        UpdateData?.Message
        ?? UpdateData?.EditedMessage
        ?? UpdateData?.ChannelPost
        ?? UpdateData?.EditedChannelPost
        ?? UpdateData?.CallbackQuery?.Message;

    /// <summary>
    ///     Is this an action ? (i.e. button click)
    /// </summary>
    public bool IsAction { get; private set; }

    /// <summary>
    ///     Is this a command ? Starts with a slash '/' and a command
    /// </summary>
    public bool IsBotCommand { get; private set; }

    /// <summary>
    /// Is this a bot command sent from a group via @BotId ?
    /// </summary>
    public bool IsBotGroupCommand { get; private set; }

    /// <summary>
    ///     Returns a List of all parameters which has been sent with the command itself (i.e. /start 123 456 789 =>
    ///     123,456,789)
    /// </summary>
    public List<string> BotCommandParameters
    {
        get
        {
            if (!IsBotCommand)
            {
                return new List<string>();
            }

            //Split by empty space and skip first entry (command itself), return as list
            return MessageText.Split(' ').Skip(1).ToList();
        }
    }

    /// <summary>
    ///     Returns just the command (i.e. /start 1 2 3 => /start)
    /// </summary>
    public string BotCommand { get; private set; }

    /// <summary>
    ///     Returns if this message will be used on the first form or not.
    /// </summary>
    public bool IsFirstHandler { get; set; } = true;

    public bool Handled { get; set; } = false;

    public string RawData => UpdateData?.CallbackQuery?.Data;

    public T GetData<T>()
        where T : class
    {
        T cd = null;
        try
        {
            cd = JsonConvert.DeserializeObject<T>(RawData);

            return cd;
        }
        catch
        {
        }

        return null;
    }

    /// <summary>
    ///     Confirm incoming action (i.e. Button click)
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task ConfirmAction(string message = "", bool showAlert = false, string urlToOpen = null)
    {
        await Device.ConfirmAction(UpdateData.CallbackQuery.Id, message, showAlert, urlToOpen);
    }

    public override async Task DeleteMessage()
    {
        try
        {
            await base.DeleteMessage(MessageId);
        }
        catch
        {
        }
    }
}
