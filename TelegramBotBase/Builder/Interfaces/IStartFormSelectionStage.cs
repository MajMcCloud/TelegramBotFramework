using System;
using System.Collections.Generic;
using System.Text;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Builder.Interfaces
{
    public interface IStartFormSelectionStage
    {

        /// <summary>
        /// Chooses a start form type which will be used for new sessions.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        INetworkingSelectionStage WithStartForm(Type startFormClass);

        /// <summary>
        /// Chooses a generic start form which will be used for new sessions.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        INetworkingSelectionStage WithStartForm<T>() where T : FormBase, new();

        /// <summary>
        /// Chooses a StartFormFactory which will be use for new sessions.
        /// </summary>
        /// <param name="startFormClass"></param>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        INetworkingSelectionStage WithServiceProvider(Type startFormClass, IServiceProvider serviceProvider);

        /// <summary>
        /// Chooses a StartFormFactory which will be use for new sessions.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        INetworkingSelectionStage WithServiceProvider<T>(IServiceProvider serviceProvider) where T : FormBase;

        /// <summary>
        /// Chooses a StartFormFactory which will be use for new sessions.
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        INetworkingSelectionStage WithStartFormFactory(IStartFormFactory factory);

    }
}
