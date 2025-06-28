﻿namespace TelegramBotBase.Enums;

public enum ESettings
{
    /// <summary>
    ///     How often could a form navigate to another (within one user action/call/message)
    /// </summary>
    NavigationMaximum = 1,


    /// <summary>
    ///     Loggs all messages and sent them to the event handler
    /// </summary>
    LogAllMessages = 2,


    /// <summary>
    ///     Skips all messages during running (good for big delay updates)
    /// </summary>
    SkipAllMessages = 3,


    /// <summary>
    ///     Does stick to the console event handler and saves all sessions on exit.
    /// </summary>
    SaveSessionsOnConsoleExit = 4,


    /// <summary>
    ///     Indicates the maximum number of times a request that received error
    ///     429 will be sent again after a timeout until it receives code 200 or an error code not equal to 429.
    /// </summary>
    MaxNumberOfRetries = 5,


    /// <summary>
    ///    Indicates if the bot should handle bot relation changes and call the eventhandler.
    /// </summary>
    HandleRelationChanges = 6,
}