using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using TelegramBotBase.Form;
using Telegram.Bot;
using TelegramBotBase.Base;
using TelegramBotBase.Args;
using TelegramBotBase.RequestDispatchers;

namespace TelegramBotBase.Interfaces
{
    public interface IDeviceSessionMethods
    {

        string GetChatTitle();

        Task BanUser(long userId, DateTime until = default,
            RequestPriority priority = RequestPriority.Normal);

        Task UnbanUser(long userId,
            RequestPriority priority = RequestPriority.Normal);

        Task ChangeChatPermissions(ChatPermissions permissions,
            RequestPriority priority = RequestPriority.Normal);

        Task RestrictUser(long userId, ChatPermissions permissions, bool useIndependentGroupPermission = false, DateTime until = default,
            RequestPriority priority = RequestPriority.Normal);

        Task ConfirmAction(string callbackQueryId, string message = "", bool showAlert = false,
                                    string urlToOpen = null,
            RequestPriority priority = RequestPriority.Critical);

        Task<bool> DeleteMessage(int messageId = -1,
            RequestPriority priority = RequestPriority.UI);

        Task<bool> DeleteMessage(Message message,
            RequestPriority priority = RequestPriority.UI);

        Task<Message> HideReplyKeyboard(string closedMsg = "Closed", bool autoDeleteResponse = true,
            RequestPriority priority = RequestPriority.UI);

        Task<Message> Send(string text, ButtonForm buttons = null, int replyTo = 0,
                                        bool disableNotification = false, ParseMode parseMode = ParseMode.Markdown,
                                        bool markdownV2AutoEscape = true,
            RequestPriority priority = RequestPriority.Normal);

        Task<Message> Send(string text, ReplyMarkup markup, int replyTo = 0,
                                        bool disableNotification = false, ParseMode parseMode = ParseMode.Markdown,
                                        bool markdownV2AutoEscape = true,
            RequestPriority priority = RequestPriority.Normal);

        Task<Message> Send(string text, InlineKeyboardMarkup markup, int replyTo = 0,
                                    bool disableNotification = false, ParseMode parseMode = ParseMode.Markdown,
                                    bool markdownV2AutoEscape = true,
            RequestPriority priority = RequestPriority.Normal);

        Task SetAction(ChatAction action,
            RequestPriority priority = RequestPriority.UI);

        Task<Message> SendTextFile(string filename, string textcontent, Encoding encoding = null,
                                            string caption = "", ButtonForm buttons = null, int replyTo = 0,
                                            bool disableNotification = false,
            RequestPriority priority = RequestPriority.Normal);

        Task<Message> SendDocument(InputFile document, string caption = "",
                                            ButtonForm buttons = null, int replyTo = 0,
                                            bool disableNotification = false,
            RequestPriority priority = RequestPriority.Normal);


        Task<Message> SendPhoto(InputFile file, string caption = null, ButtonForm buttons = null,
                                         int replyTo = 0, bool disableNotification = false,
                                         ParseMode parseMode = ParseMode.Markdown,
            RequestPriority priority = RequestPriority.Normal);

        Task<Message> SendVideo(InputFile file, string caption = null, ButtonForm buttons = null,
                                         int replyTo = 0, bool disableNotification = false,
                                         ParseMode parseMode = ParseMode.Markdown,
            RequestPriority priority = RequestPriority.Normal);


        Task<Message> SendVideo(string url, ButtonForm buttons = null, int replyTo = 0,
                                         bool disableNotification = false, ParseMode parseMode = ParseMode.Markdown,
            RequestPriority priority = RequestPriority.Normal);

        Task<Message> SendVideo(string filename, byte[] video, ButtonForm buttons = null, int replyTo = 0,
                                         bool disableNotification = false, ParseMode parseMode = ParseMode.Markdown,
            RequestPriority priority = RequestPriority.Normal);

        Task<Message> SendLocalVideo(string filepath, ButtonForm buttons = null, int replyTo = 0,
                                              bool disableNotification = false,
                                              ParseMode parseMode = ParseMode.Markdown,
            RequestPriority priority = RequestPriority.Normal);

        Task<Message> Edit(int messageId, string text, ButtonForm buttons = null,
                                    ParseMode parseMode = ParseMode.Markdown, bool markdownV2AutoEscape = false,
            RequestPriority priority = RequestPriority.UI);



        Task<Message> Edit(int messageId, string text, InlineKeyboardMarkup markup,
                                    ParseMode parseMode = ParseMode.Markdown, bool markdownV2AutoEscape = false,
            RequestPriority priority = RequestPriority.UI);

        Task<Message> Edit(Message message, ButtonForm buttons = null,
                                    ParseMode parseMode = ParseMode.Markdown, bool markdownV2AutoEscape = false,
            RequestPriority priority = RequestPriority.UI);

        Task<Message> EditReplyMarkup(int messageId, ButtonForm bf,
            RequestPriority priority = RequestPriority.UI);

        Task<Message> RequestContact(string buttonText = "Send your contact",
                                              string requestMessage = "Give me your phone number!",
                                              bool oneTimeOnly = true,
            RequestPriority priority = RequestPriority.Normal);

        Task<Message> RequestLocation(string buttonText = "Send your location",
                                               string requestMessage = "Give me your location!",
                                               bool oneTimeOnly = true,
            RequestPriority priority = RequestPriority.Normal);


        Task<ChatMember> GetChatUser(long userId,
            RequestPriority priority = RequestPriority.Normal);


        event Async.AsyncEventHandler<MessageSentEventArgs> MessageSent;

        event EventHandler<MessageReceivedEventArgs> MessageReceived;

        event EventHandler<MessageDeletedEventArgs> MessageDeleted;

        Task Api(Func<ITelegramBotClient, Task> call);

        Task<T> Api<T>(Func<ITelegramBotClient, Task<T>> call);

        T Raw<T>(Func<ITelegramBotClient, T> call);

    }
}
