using System;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Builder.Interfaces;

public interface IAPIKeySelectionStage
{
    /// <summary>
    ///     Sets the API Key which will be used by the telegram bot client.
    /// </summary>
    /// <param name="apiKey"></param>
    /// <returns></returns>
    IMessageLoopSelectionStage WithAPIKey(string apiKey);


    /// <summary>
    ///     Quick and easy way to create a BotBase instance.
    ///     Uses: DefaultMessageLoop, NoProxy, OnlyStart, NoSerialization, DefaultLanguage
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="StartForm"></param>
    /// <returns></returns>
    IBuildingStage QuickStart(string apiKey, Type StartForm);

    /// <summary>
    ///     Quick and easy way to create a BotBase instance.
    ///     Uses: DefaultMessageLoop, NoProxy, OnlyStart, NoSerialization, DefaultLanguage
    /// </summary>
    /// <param name="apiKey"></param>
    /// <returns></returns>
    IBuildingStage QuickStart<T>(string apiKey) where T : FormBase;

    /// <summary>
    ///     Quick and easy way to create a BotBase instance.
    ///     Uses: DefaultMessageLoop, NoProxy, OnlyStart, NoSerialization, DefaultLanguage
    /// </summary>
    /// <param name="apiKey"></param>
    /// <param name="StartFormFactory"></param>
    /// <returns></returns>
    IBuildingStage QuickStart(string apiKey, IStartFormFactory StartFormFactory);
}