using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Exceptions;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Markdown;

namespace TelegramBotBase.Sessions;

/// <summary>
///     Base class for a device/chat session
/// </summary>
public class DeviceSession : IDeviceSession
{
    private static readonly object EvMessageSent = new();
    private static readonly object EvMessageReceived = new();
    private static readonly object EvMessageDeleted = new();

    private readonly EventHandlerList _events = new();

    public DeviceSession()
    {
    }

    public DeviceSession(long deviceId)
    {
        DeviceId = deviceId;
    }

    public DeviceSession(long deviceId, FormBase startForm)
    {
        DeviceId = deviceId;
        ActiveForm = startForm;
        ActiveForm.Device = this;
    }

    /// <summary>
    ///     Returns the ID of the last received message.
    /// </summary>
    public int LastMessageId => LastMessage?.MessageId ?? -1;

    /// <summary>
    ///     Returns the last received message.
    /// </summary>
    public Message LastMessage { get; set; }

    public MessageClient Client => ActiveForm.Client;

    /// <summary>
    ///     Returns if the messages is posted within a group.
    /// </summary>
    public bool IsGroup => LastMessage != null &&
                           (LastMessage.Chat.Type == ChatType.Group) |
                           (LastMessage.Chat.Type == ChatType.Supergroup);

    /// <summary>
    ///     Returns if the messages is posted within a channel.
    /// </summary>
    public bool IsChannel => LastMessage != null && LastMessage.Chat.Type == ChatType.Channel;

    #region "Static"

    /// <summary>
    ///     Indicates the maximum number of times a request that received error
    ///     429 will be sent again after a timeout until it receives code 200 or an error code not equal to 429.
    /// </summary>
    public static uint MaxNumberOfRetries { get; set; }

    #endregion "Static"

    /// <summary>
    ///     Device or chat id
    /// </summary>
    public long DeviceId { get; set; }

    /// <summary>
    ///     Username of user or group
    /// </summary>
    public string ChatTitle { get; set; }

    /// <summary>
    ///     When did any last action happend (message received or button clicked)
    /// </summary>
    public DateTime LastAction { get; set; }

    /// <summary>
    ///     Returns the form where the user/group is at the moment.
    /// </summary>
    public FormBase ActiveForm { get; set; }

    /// <summary>
    ///     Returns the previous shown form
    /// </summary>
    public FormBase PreviousForm { get; set; }

    /// <summary>
    ///     contains if the form has been switched (navigated)
    /// </summary>
    public bool FormSwitched { get; set; } = false;

    /// <summary>
    ///     Returns the ChatTitle depending on groups/channels or users
    /// </summary>
    /// <returns></returns>
    public string GetChatTitle()
    {
        return LastMessage?.Chat.Title
               ?? LastMessage?.Chat.Username
               ?? LastMessage?.Chat.FirstName
               ?? ChatTitle;
    }


    /// <summary>
    ///     Confirm incoming action (i.e. Button click)
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task ConfirmAction(string callbackQueryId, string message = "", bool showAlert = false,
                                    string urlToOpen = null)
    {

        await Client.TelegramClient.AnswerCallbackQueryAsync(callbackQueryId, message, showAlert, urlToOpen);

    }

    /// <summary>
    ///     Edits the text message
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="text"></param>
    /// <param name="buttons"></param>
    /// <returns></returns>
    public async Task<Message> Edit(int messageId, string text, ButtonForm buttons = null,
                                    ParseMode parseMode = ParseMode.Markdown)
    {
        InlineKeyboardMarkup markup = buttons;

        if (text.Length > Constants.Telegram.MaxMessageLength)
        {
            throw new MessageTooLongException(text.Length);
        }


        return await Api(a => a.EditMessageTextAsync(DeviceId, messageId, text, parseMode, replyMarkup: markup));
    }

    /// <summary>
    ///     Edits the text message
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="text"></param>
    /// <param name="buttons"></param>
    /// <returns></returns>
    public async Task<Message> Edit(int messageId, string text, InlineKeyboardMarkup markup,
                                    ParseMode parseMode = ParseMode.Markdown)
    {
        if (text.Length > Constants.Telegram.MaxMessageLength)
        {
            throw new MessageTooLongException(text.Length);
        }


        return await Api(a => a.EditMessageTextAsync(DeviceId, messageId, text, parseMode, replyMarkup: markup));

    }

    /// <summary>
    ///     Edits the text message
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="text"></param>
    /// <param name="buttons"></param>
    /// <returns></returns>
    public async Task<Message> Edit(Message message, ButtonForm buttons = null,
                                    ParseMode parseMode = ParseMode.Markdown)
    {
        InlineKeyboardMarkup markup = buttons;

        if (message.Text.Length > Constants.Telegram.MaxMessageLength)
        {
            throw new MessageTooLongException(message.Text.Length);
        }

        return await Api(a => a.EditMessageTextAsync(DeviceId, message.MessageId, message.Text, parseMode,
                                                    replyMarkup: markup));
    }

    /// <summary>
    ///     Edits the reply keyboard markup (buttons)
    /// </summary>
    /// <param name="messageId"></param>
    /// <param name="bf"></param>
    /// <returns></returns>
    public async Task<Message> EditReplyMarkup(int messageId, ButtonForm bf)
    {

        return await Api(a => a.EditMessageReplyMarkupAsync(DeviceId, messageId, bf));
    }

    /// <summary>
    ///     Sends a simple text message
    /// </summary>
    /// <param name="text"></param>
    /// <param name="buttons"></param>
    /// <param name="replyTo"></param>
    /// <param name="disableNotification"></param>
    /// <returns></returns>
    public async Task<Message> Send(long deviceId, string text, ButtonForm buttons = null, int replyTo = 0,
                                    bool disableNotification = false, ParseMode parseMode = ParseMode.Markdown,
                                    bool markdownV2AutoEscape = true)
    {
        if (ActiveForm == null)
        {
            return null;
        }

        InlineKeyboardMarkup markup = buttons;

        if (text.Length > Constants.Telegram.MaxMessageLength)
        {
            throw new MessageTooLongException(text.Length);
        }

        if (parseMode == ParseMode.MarkdownV2 && markdownV2AutoEscape)
        {
            text = text.MarkdownV2Escape();
        }


        var t = Api(a => a.SendTextMessageAsync(deviceId, text, null, parseMode, replyToMessageId: replyTo,
                                                replyMarkup: markup, disableNotification: disableNotification));

        var o = GetOrigin(new StackTrace());

        await OnMessageSent(new MessageSentEventArgs(await t, o));

        return await t;

    }

    /// <summary>
    ///     Sends a simple text message
    /// </summary>
    /// <param name="text"></param>
    /// <param name="buttons"></param>
    /// <param name="replyTo"></param>
    /// <param name="disableNotification"></param>
    /// <returns></returns>
    public async Task<Message> Send(string text, ButtonForm buttons = null, int replyTo = 0,
                                    bool disableNotification = false, ParseMode parseMode = ParseMode.Markdown,
                                    bool markdownV2AutoEscape = true)
    {
        return await Send(DeviceId, text, buttons, replyTo, disableNotification, parseMode, markdownV2AutoEscape);
    }

    /// <summary>
    ///     Sends a simple text message
    /// </summary>
    /// <param name="text"></param>
    /// <param name="markup"></param>
    /// <param name="replyTo"></param>
    /// <param name="disableNotification"></param>
    /// <returns></returns>
    public async Task<Message> Send(string text, InlineKeyboardMarkup markup, int replyTo = 0,
                                    bool disableNotification = false, ParseMode parseMode = ParseMode.Markdown,
                                    bool markdownV2AutoEscape = true)
    {
        if (ActiveForm == null)
        {
            return null;
        }

        if (text.Length > Constants.Telegram.MaxMessageLength)
        {
            throw new MessageTooLongException(text.Length);
        }

        if (parseMode == ParseMode.MarkdownV2 && markdownV2AutoEscape)
        {
            text = text.MarkdownV2Escape();
        }


        var t = Api(a => a.SendTextMessageAsync(DeviceId, text, null, parseMode, replyToMessageId: replyTo,
                                                replyMarkup: markup, disableNotification: disableNotification));

        var o = GetOrigin(new StackTrace());
        await OnMessageSent(new MessageSentEventArgs(await t, o));

        return await t;

    }

    /// <summary>
    ///     Sends a simple text message
    /// </summary>
    /// <param name="text"></param>
    /// <param name="markup"></param>
    /// <param name="replyTo"></param>
    /// <param name="disableNotification"></param>
    /// <returns></returns>
    public async Task<Message> Send(string text, ReplyMarkupBase markup, int replyTo = 0,
                                    bool disableNotification = false, ParseMode parseMode = ParseMode.Markdown,
                                    bool markdownV2AutoEscape = true)
    {
        if (ActiveForm == null)
        {
            return null;
        }

        if (text.Length > Constants.Telegram.MaxMessageLength)
        {
            throw new MessageTooLongException(text.Length);
        }

        if (parseMode == ParseMode.MarkdownV2 && markdownV2AutoEscape)
        {
            text = text.MarkdownV2Escape();
        }


        var t = Api(a => a.SendTextMessageAsync(DeviceId, text, null, parseMode, replyToMessageId: replyTo,
                                                replyMarkup: markup, disableNotification: disableNotification));

        var o = GetOrigin(new StackTrace());
        await OnMessageSent(new MessageSentEventArgs(await t, o));

        return await t;

    }

    /// <summary>
    ///     Sends an image
    /// </summary>
    /// <param name="file"></param>
    /// <param name="buttons"></param>
    /// <param name="replyTo"></param>
    /// <param name="disableNotification"></param>
    /// <returns></returns>
    public async Task<Message> SendPhoto(InputFile file, string caption = null, ButtonForm buttons = null,
                                         int replyTo = 0, bool disableNotification = false,
                                         ParseMode parseMode = ParseMode.Markdown)
    {
        if (ActiveForm == null)
        {
            return null;
        }

        InlineKeyboardMarkup markup = buttons;


        var t = Api(a => a.SendPhotoAsync(DeviceId, file, null, caption, parseMode, replyToMessageId: replyTo,
                                          replyMarkup: markup, disableNotification: disableNotification));

        var o = GetOrigin(new StackTrace());
        await OnMessageSent(new MessageSentEventArgs(await t, o));

        return await t;

    }

    /// <summary>
    ///     Sends an video
    /// </summary>
    /// <param name="file"></param>
    /// <param name="buttons"></param>
    /// <param name="replyTo"></param>
    /// <param name="disableNotification"></param>
    /// <returns></returns>
    public async Task<Message> SendVideo(InputFile file, string caption = null, ButtonForm buttons = null,
                                         int replyTo = 0, bool disableNotification = false,
                                         ParseMode parseMode = ParseMode.Markdown)
    {
        if (ActiveForm == null)
        {
            return null;
        }

        InlineKeyboardMarkup markup = buttons;


        var t = Api(a => a.SendVideoAsync(DeviceId, file, caption: caption, parseMode: parseMode,
                                          replyToMessageId: replyTo, replyMarkup: markup,
                                          disableNotification: disableNotification));

        var o = GetOrigin(new StackTrace());
        await OnMessageSent(new MessageSentEventArgs(await t, o));

        return await t;

    }

    /// <summary>
    ///     Sends an video
    /// </summary>
    /// <param name="url"></param>
    /// <param name="buttons"></param>
    /// <param name="replyTo"></param>
    /// <param name="disableNotification"></param>
    /// <returns></returns>
    public async Task<Message> SendVideo(string url, ButtonForm buttons = null, int replyTo = 0,
                                         bool disableNotification = false, ParseMode parseMode = ParseMode.Markdown)
    {
        if (ActiveForm == null)
        {
            return null;
        }

        InlineKeyboardMarkup markup = buttons;


        var t = Api(a => a.SendVideoAsync(DeviceId, InputFile.FromUri(url), parseMode: parseMode,
                                          replyToMessageId: replyTo, replyMarkup: markup,
                                          disableNotification: disableNotification));

        var o = GetOrigin(new StackTrace());
        await OnMessageSent(new MessageSentEventArgs(await t, o));

        return await t;

    }

    /// <summary>
    ///     Sends an video
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="video"></param>
    /// <param name="buttons"></param>
    /// <param name="replyTo"></param>
    /// <param name="disableNotification"></param>
    /// <returns></returns>
    public async Task<Message> SendVideo(string filename, byte[] video, ButtonForm buttons = null, int replyTo = 0,
                                         bool disableNotification = false, ParseMode parseMode = ParseMode.Markdown)
    {
        if (ActiveForm == null)
        {
            return null;
        }

        InlineKeyboardMarkup markup = buttons;


        var ms = new MemoryStream(video);

        var fts = InputFile.FromStream(ms, filename);

        var t = Api(a => a.SendVideoAsync(DeviceId, fts, parseMode: parseMode, replyToMessageId: replyTo,
                                          replyMarkup: markup, disableNotification: disableNotification));

        var o = GetOrigin(new StackTrace());
        await OnMessageSent(new MessageSentEventArgs(await t, o));

        return await t;

    }

    /// <summary>
    ///     Sends an local file as video
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="video"></param>
    /// <param name="buttons"></param>
    /// <param name="replyTo"></param>
    /// <param name="disableNotification"></param>
    /// <returns></returns>
    public async Task<Message> SendLocalVideo(string filepath, ButtonForm buttons = null, int replyTo = 0,
                                              bool disableNotification = false,
                                              ParseMode parseMode = ParseMode.Markdown)
    {
        if (ActiveForm == null)
        {
            return null;
        }

        InlineKeyboardMarkup markup = buttons;


        var fs = new FileStream(filepath, FileMode.Open);

        var filename = Path.GetFileName(filepath);

        var fts = InputFile.FromStream(fs, filename);

        var t = Api(a => a.SendVideoAsync(DeviceId, fts, parseMode: parseMode, replyToMessageId: replyTo,
                                          replyMarkup: markup, disableNotification: disableNotification));

        var o = GetOrigin(new StackTrace());
        await OnMessageSent(new MessageSentEventArgs(await t, o));

        return await t;

    }

    /// <summary>
    ///     Sends an document
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="document"></param>
    /// <param name="caption"></param>
    /// <param name="buttons"></param>
    /// <param name="replyTo"></param>
    /// <param name="disableNotification"></param>
    /// <returns></returns>
    public async Task<Message> SendDocument(string filename, byte[] document, string caption = "",
                                            ButtonForm buttons = null, int replyTo = 0,
                                            bool disableNotification = false)
    {
        var ms = new MemoryStream(document);

        var fts = InputFile.FromStream(ms, filename);

        return await SendDocument(fts, caption, buttons, replyTo, disableNotification);
    }

    /// <summary>
    ///     Generates a Textfile from scratch with the specified encoding. (Default is UTF8)
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="textcontent"></param>
    /// <param name="encoding">Default is UTF8</param>
    /// <param name="caption"></param>
    /// <param name="buttons"></param>
    /// <param name="replyTo"></param>
    /// <param name="disableNotification"></param>
    /// <returns></returns>
    public async Task<Message> SendTextFile(string filename, string textcontent, Encoding encoding = null,
                                            string caption = "", ButtonForm buttons = null, int replyTo = 0,
                                            bool disableNotification = false)
    {
        encoding = encoding ?? Encoding.UTF8;

        var ms = new MemoryStream();
        var sw = new StreamWriter(ms, encoding);

        sw.Write(textcontent);
        sw.Flush();

        var content = ms.ToArray();

        return await SendDocument(filename, content, caption, buttons, replyTo, disableNotification);
    }

    /// <summary>
    ///     Sends an document
    /// </summary>
    /// <param name="document"></param>
    /// <param name="caption"></param>
    /// <param name="buttons"></param>
    /// <param name="replyTo"></param>
    /// <param name="disableNotification"></param>
    /// <returns></returns>
    public async Task<Message> SendDocument(InputFile document, string caption = "",
                                            ButtonForm buttons = null, int replyTo = 0,
                                            bool disableNotification = false)
    {
        InlineKeyboardMarkup markup = null;
        if (buttons != null)
        {
            markup = buttons;
        }


        var t = Api(a => a.SendDocumentAsync(DeviceId, document, null, null, caption, replyMarkup: markup,
                                             disableNotification: disableNotification, replyToMessageId: replyTo));

        var o = GetOrigin(new StackTrace());
        await OnMessageSent(new MessageSentEventArgs(await t, o));

        return await t;

    }

    /// <summary>
    ///     Set a chat action (showed to the user)
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public async Task SetAction(ChatAction action)
    {
        await Api(a => a.SendChatActionAsync(DeviceId, action));
    }

    /// <summary>
    ///     Requests the contact from the user.
    /// </summary>
    /// <param name="buttonText"></param>
    /// <param name="requestMessage"></param>
    /// <param name="oneTimeOnly"></param>
    /// <returns></returns>
    public async Task<Message> RequestContact(string buttonText = "Send your contact",
                                              string requestMessage = "Give me your phone number!",
                                              bool oneTimeOnly = true)
    {
        var rck = new ReplyKeyboardMarkup(KeyboardButton.WithRequestContact(buttonText))
        {
            OneTimeKeyboard = oneTimeOnly
        };
        return await Api(a => a.SendTextMessageAsync(DeviceId, requestMessage, replyMarkup: rck));
    }

    /// <summary>
    ///     Requests the location from the user.
    /// </summary>
    /// <param name="buttonText"></param>
    /// <param name="requestMessage"></param>
    /// <param name="oneTimeOnly"></param>
    /// <returns></returns>
    public async Task<Message> RequestLocation(string buttonText = "Send your location",
                                               string requestMessage = "Give me your location!",
                                               bool oneTimeOnly = true)
    {
        var rcl = new ReplyKeyboardMarkup(KeyboardButton.WithRequestLocation(buttonText))
        {
            OneTimeKeyboard = oneTimeOnly
        };
        return await Api(a => a.SendTextMessageAsync(DeviceId, requestMessage, replyMarkup: rcl));
    }

    public async Task<Message> HideReplyKeyboard(string closedMsg = "Closed", bool autoDeleteResponse = true)
    {

        var m = await Send(closedMsg, new ReplyKeyboardRemove());

        if (autoDeleteResponse && m != null)
        {
            await DeleteMessage(m);
        }

        return m;

    }

    /// <summary>
    ///     Deletes a message
    /// </summary>
    /// <param name="messageId"></param>
    /// <returns></returns>
    public virtual async Task<bool> DeleteMessage(int messageId = -1)
    {
        await Raw(a => a.DeleteMessageAsync(DeviceId, messageId));

        OnMessageDeleted(new MessageDeletedEventArgs(messageId));

        return true;
    }

    /// <summary>
    ///     Deletes the given message
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public virtual async Task<bool> DeleteMessage(Message message)
    {
        return await DeleteMessage(message.MessageId);
    }


    public virtual async Task ChangeChatPermissions(ChatPermissions permissions)
    {

        await Api(a => a.SetChatPermissionsAsync(DeviceId, permissions));

    }

    private Type GetOrigin(StackTrace stackTrace)
    {
        for (var i = 0; i < stackTrace.FrameCount; i++)
        {
            var methodBase = stackTrace.GetFrame(i).GetMethod();

            //Debug.WriteLine(methodBase.Name);

            if (methodBase.DeclaringType.IsSubclassOf(typeof(FormBase)) |
                methodBase.DeclaringType.IsSubclassOf(typeof(ControlBase)))
            {
                return methodBase.DeclaringType;
            }
        }

        return null;
    }

    /// <summary>
    ///     Gives access to the original TelegramClient without any Exception catchings.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="call"></param>
    /// <returns></returns>
    public T Raw<T>(Func<ITelegramBotClient, T> call)
    {
        return call(Client.TelegramClient);
    }

    /// <summary>
    ///     This will call a function on the TelegramClient and automatically Retry if an limit has been exceeded.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="call"></param>
    /// <returns></returns>
    public async Task<T> Api<T>(Func<ITelegramBotClient, Task<T>> call)
    {
        var numberOfTries = 0;
        while (numberOfTries < MaxNumberOfRetries)
        {
            try
            {
                return await call(Client.TelegramClient);
            }
            catch (ApiRequestException ex)
            {
                if (ex.ErrorCode != 429)
                {
                    throw;
                }

                if (ex.Parameters != null && ex.Parameters.RetryAfter != null)
                {
                    await Task.Delay(ex.Parameters.RetryAfter.Value * 1000);
                }

                numberOfTries++;
            }
        }

        return default;
    }

    /// <summary>
    ///     This will call a function on the TelegramClient and automatically Retry if an limit has been exceeded.
    /// </summary>
    /// <param name="call"></param>
    /// <returns></returns>
    public async Task Api(Func<ITelegramBotClient, Task> call)
    {
        var numberOfTries = 0;
        while (numberOfTries < MaxNumberOfRetries)
        {
            try
            {
                await call(Client.TelegramClient);
                return;
            }
            catch (ApiRequestException ex)
            {
                if (ex.ErrorCode != 429)
                {
                    throw;
                }

                if (ex.Parameters != null && ex.Parameters.RetryAfter != null)
                {
                    await Task.Delay(ex.Parameters.RetryAfter.Value * 1000);
                }

                numberOfTries++;
            }
        }
    }

    #region "Users"

    public virtual async Task RestrictUser(long userId, ChatPermissions permissions, bool? useIndependentGroupPermission = null, DateTime until = default)
    {
        try
        {
            await Api(a => a.RestrictChatMemberAsync(DeviceId, userId, permissions, useIndependentGroupPermission, until));
        }
        catch
        {
        }
    }

    public virtual async Task<ChatMember> GetChatUser(long userId)
    {

        return await Api(a => a.GetChatMemberAsync(DeviceId, userId));

    }

    [Obsolete("User BanUser instead.")]
    public virtual async Task KickUser(long userId, DateTime until = default)
    {

        await Api(a => a.BanChatMemberAsync(DeviceId, userId, until));

    }

    public virtual async Task BanUser(long userId, DateTime until = default)
    {

        await Api(a => a.BanChatMemberAsync(DeviceId, userId, until));

    }

    public virtual async Task UnbanUser(long userId)
    {

        await Api(a => a.UnbanChatMemberAsync(DeviceId, userId));

    }

    #endregion


    #region "Events"

    /// <summary>
    ///     Eventhandler for sent messages
    /// </summary>
    public event Async.AsyncEventHandler<MessageSentEventArgs> MessageSent
    {
        add => _events.AddHandler(EvMessageSent, value);
        remove => _events.RemoveHandler(EvMessageSent, value);
    }


    public async Task OnMessageSent(MessageSentEventArgs e)
    {
        if (e.Message == null)
        {
            return;
        }

        var handler = _events[EvMessageSent]?.GetInvocationList()
                                            .Cast<Async.AsyncEventHandler<MessageSentEventArgs>>();
        if (handler == null)
        {
            return;
        }

        foreach (var h in handler)
        {
            await h.InvokeAllAsync(this, e);
        }

        //(this.__Events[__evMessageSent] as EventHandler<MessageSentEventArgs>)?.Invoke(this, e);
    }

    /// <summary>
    ///     Eventhandler for received messages
    /// </summary>
    public event EventHandler<MessageReceivedEventArgs> MessageReceived
    {
        add => _events.AddHandler(EvMessageReceived, value);
        remove => _events.RemoveHandler(EvMessageReceived, value);
    }


    public void OnMessageReceived(MessageReceivedEventArgs e)
    {
        (_events[EvMessageReceived] as EventHandler<MessageReceivedEventArgs>)?.Invoke(this, e);
    }

    /// <summary>
    ///     Eventhandler for deleting messages
    /// </summary>
    public event EventHandler<MessageDeletedEventArgs> MessageDeleted
    {
        add => _events.AddHandler(EvMessageDeleted, value);
        remove => _events.RemoveHandler(EvMessageDeleted, value);
    }


    public void OnMessageDeleted(MessageDeletedEventArgs e)
    {
        (_events[EvMessageDeleted] as EventHandler<MessageDeletedEventArgs>)?.Invoke(this, e);
    }

    #endregion
}
