﻿using TelegramBotBase.Interfaces;
using TelegramBotBase.Interfaces.ExternalActions;

namespace TelegramBotBase.Builder.Interfaces;

public interface IMessageLoopSelectionStage
{
    /// <summary>
    ///     Chooses a default message loop.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IStartFormSelectionStage DefaultMessageLoop(IExternalActionManager managerInstance = null);


    /// <summary>
    ///     Chooses a minimalistic message loop, which catches all update types and only calls the Load function.
    /// </summary>
    /// <returns></returns>
    IStartFormSelectionStage MinimalMessageLoop();


    /// <summary>
    ///     Chooses a custom message loop.
    /// </summary>
    /// <param name="startFormClass"></param>
    /// <returns></returns>
    IStartFormSelectionStage CustomMessageLoop(IMessageLoopFactory startFormClass);


    /// <summary>
    ///     Chooses a custom message loop.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    IStartFormSelectionStage CustomMessageLoop<T>() where T : class, new();
}