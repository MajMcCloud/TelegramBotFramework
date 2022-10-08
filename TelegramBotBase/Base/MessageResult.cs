using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBotBase.Base;

public class MessageResult : ResultBase
{
    internal MessageResult()
    {
    }

    public MessageResult(Update update)
    {
        UpdateData = update;
    }

    public Update UpdateData { get; set; }

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
    public bool IsAction => UpdateData.CallbackQuery != null;

    /// <summary>
    ///     Is this a command ? Starts with a slash '/' and a command
    /// </summary>
    public bool IsBotCommand => MessageText.StartsWith("/");

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
    public string BotCommand
    {
        get
        {
            if (!IsBotCommand)
            {
                return null;
            }

            return MessageText.Split(' ')[0];
        }
    }

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
    ///     Confirm incomming action (i.e. Button click)
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
