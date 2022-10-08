using System;
using TelegramBotBase.Form;

namespace TelegramBotBase.Interfaces;

internal interface IDeviceSession
{
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