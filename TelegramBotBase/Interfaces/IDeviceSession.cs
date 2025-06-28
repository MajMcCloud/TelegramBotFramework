using System;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using TelegramBotBase.Form;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using Telegram.Bot;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Interfaces;

public interface IDeviceSession : IDeviceSessionMethods
{
    MessageClient Client => ActiveForm.Client;

    /// <summary>
    ///     Returns if the messages is posted within a group.
    /// </summary>
    bool IsGroup { get;}

    /// <summary>
    ///     Returns if the messages is posted within a channel.
    /// </summary>
    bool IsChannel { get; }

    int LastMessageId => LastMessage?.MessageId ?? -1;

    Message LastMessage { get; set; }

    /// <summary>
    ///     Device or chat id
    /// </summary>
    long DeviceId { get; set; }

    /// <summary>
    ///     Username of user or group
    /// </summary>
    string ChatTitle { get; set; }


    /// <summary>
    ///     When did any last action happend (message received or button clicked)
    /// </summary>
    DateTime LastAction { get; set; }

    /// <summary>
    ///     Returns the form where the user/group is at the moment.
    /// </summary>
    FormBase ActiveForm { get; set; }

    /// <summary>
    ///     Returns the previous shown form
    /// </summary>
    FormBase PreviousForm { get; set; }

    /// <summary>
    ///     contains if the form has been switched (navigated)
    /// </summary>
    bool FormSwitched { get; set; }


}