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

namespace TelegramBotBase.Interfaces
{
    public interface IDeviceSessionMethods
    {

        string GetChatTitle();

        Task BanUser(long userId, DateTime until = default);

        Task UnbanUser(long userId);

        Task ChangeChatPermissions(ChatPermissions permissions);

        Task RestrictUser(long userId, ChatPermissions permissions, bool? useIndependentGroupPermission = null, DateTime until = default);

        Task ConfirmAction(string callbackQueryId, string message = "", bool showAlert = false,
                                    string urlToOpen = null);

        Task<bool> DeleteMessage(int messageId = -1);

        Task<bool> DeleteMessage(Message message);

        Task<Message> HideReplyKeyboard(string closedMsg = "Closed", bool autoDeleteResponse = true);

        Task<Message> Send(string text, ButtonForm buttons = null, int replyTo = 0,
                                        bool disableNotification = false, ParseMode parseMode = ParseMode.Markdown,
                                        bool markdownV2AutoEscape = true);

        Task<Message> Send(string text, IReplyMarkup markup, int replyTo = 0,
                                        bool disableNotification = false, ParseMode parseMode = ParseMode.Markdown,
                                        bool markdownV2AutoEscape = true);

        Task<Message> Send(string text, InlineKeyboardMarkup markup, int replyTo = 0,
                                    bool disableNotification = false, ParseMode parseMode = ParseMode.Markdown,
                                    bool markdownV2AutoEscape = true);

        Task SetAction(ChatAction action);

        Task<Message> SendTextFile(string filename, string textcontent, Encoding encoding = null,
                                            string caption = "", ButtonForm buttons = null, int replyTo = 0,
                                            bool disableNotification = false);

        Task<Message> SendDocument(InputFile document, string caption = "",
                                            ButtonForm buttons = null, int replyTo = 0,
                                            bool disableNotification = false);


        Task<Message> SendPhoto(InputFile file, string caption = null, ButtonForm buttons = null,
                                         int replyTo = 0, bool disableNotification = false,
                                         ParseMode parseMode = ParseMode.Markdown);

        Task<Message> SendVideo(InputFile file, string caption = null, ButtonForm buttons = null,
                                         int replyTo = 0, bool disableNotification = false,
                                         ParseMode parseMode = ParseMode.Markdown);


        Task<Message> SendVideo(string url, ButtonForm buttons = null, int replyTo = 0,
                                         bool disableNotification = false, ParseMode parseMode = ParseMode.Markdown);

        Task<Message> SendVideo(string filename, byte[] video, ButtonForm buttons = null, int replyTo = 0,
                                         bool disableNotification = false, ParseMode parseMode = ParseMode.Markdown);

        Task<Message> SendLocalVideo(string filepath, ButtonForm buttons = null, int replyTo = 0,
                                              bool disableNotification = false,
                                              ParseMode parseMode = ParseMode.Markdown);

        Task<Message> Edit(int messageId, string text, ButtonForm buttons = null,
                                    ParseMode parseMode = ParseMode.Markdown);



        Task<Message> Edit(int messageId, string text, InlineKeyboardMarkup markup,
                                    ParseMode parseMode = ParseMode.Markdown);

        Task<Message> Edit(Message message, ButtonForm buttons = null,
                                    ParseMode parseMode = ParseMode.Markdown);

        Task<Message> EditReplyMarkup(int messageId, ButtonForm bf);

        Task<Message> RequestContact(string buttonText = "Send your contact",
                                              string requestMessage = "Give me your phone number!",
                                              bool oneTimeOnly = true);

        Task<Message> RequestLocation(string buttonText = "Send your location",
                                               string requestMessage = "Give me your location!",
                                               bool oneTimeOnly = true);


        Task<ChatMember> GetChatUser(long userId);


        event Async.AsyncEventHandler<MessageSentEventArgs> MessageSent;

        event EventHandler<MessageReceivedEventArgs> MessageReceived;

        event EventHandler<MessageDeletedEventArgs> MessageDeleted;

        Task Api(Func<ITelegramBotClient, Task> call);

        Task<T> Api<T>(Func<ITelegramBotClient, Task<T>> call);

        T Raw<T>(Func<ITelegramBotClient, T> call);

    }
}
