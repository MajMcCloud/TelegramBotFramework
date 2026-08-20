using System;
using TelegramBotBase.Base;
using TelegramBotBase.RequestDispatchers;

namespace TelegramBotBase.Builder.Interfaces;

public interface IRequestDispatcherSelectionStage
{
    /// <summary>Configures the bot to use <see cref="DefaultRequestDispatcher"/> (concurrency + 429 retry only)</summary>
    /// <param name="settings">Dispatcher configuration; defaults are used if omitted</param>
    /// <returns>The next builder stage</returns>
    IBotCommandsStage UseDefaultRequestDispatcher(RequestDispatcherSettings settings = null);
    
    /// <summary>Configures the bot to use <see cref="FullRequestDispatcher"/> (adds global/per-chat/per-group rate limiting)</summary>
    /// <param name="settings">Dispatcher configuration; defaults are used if omitted</param>
    /// <returns>The next builder stage</returns>
    IBotCommandsStage UseFullRequestDispatcher(RequestDispatcherSettings settings = null);

    /// <summary>Configures the bot to use a custom dispatcher built from the given <see cref="MessageClient"/></summary>
    /// <param name="factory">Factory that creates the dispatcher from the bot's message client</param>
    /// <returns>The next builder stage</returns>
    IBotCommandsStage UseCustomRequestDispatcher(Func<MessageClient, IRequestDispatcher> factory);
    
    /// <summary>Configures the bot to use a pre-built custom dispatcher instance</summary>
    /// <param name="dispatcher">The dispatcher instance to use</param>
    /// <returns>The next builder stage</returns>
    IBotCommandsStage UseCustomRequestDispatcher(IRequestDispatcher dispatcher);
}