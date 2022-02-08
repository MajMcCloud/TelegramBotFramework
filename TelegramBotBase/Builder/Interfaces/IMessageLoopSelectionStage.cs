using System;
using System.Collections.Generic;
using System.Text;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Builder.Interfaces
{
    public interface IMessageLoopSelectionStage
    {

        /// <summary>
        /// Chooses a default message loop.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IStartFormSelectionStage DefaultMessageLoop();

        /// <summary>
        /// Chooses a custom message loop.
        /// </summary>
        /// <param name="startFormClass"></param>
        /// <returns></returns>
        IStartFormSelectionStage CustomMessageLoop(IMessageLoopFactory startFormClass);


        /// <summary>
        /// Chooses a custom message loop.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IStartFormSelectionStage CustomMessageLoop<T>() where T : class, new();


    }
}
