using System;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Builder.Interfaces;

public interface ISessionSerializationStage
{
    /// <summary>
    ///     Do not uses serialization.
    /// </summary>
    /// <returns></returns>
    ILanguageSelectionStage NoSerialization();


    /// <summary>
    ///     Sets the state machine for serialization.
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    ILanguageSelectionStage UseSerialization(IStateMachine machine);

    /// <summary>
    ///     Using the complex version of .Net JSON, which can serialize all objects.
    ///     Saves in application directory.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Has been changed lately to not use Newtonsoft.Json anymore.
    ///     For the legacy version add the nuget package below and use the <seealso cref="UseNewtonsoftJson"></seealso> method.
    ///     </para>
    ///     <seealso href="https://www.nuget.org/packages/TelegramBotBase.Extensions.Serializer.Legacy.NewtonsoftJson/">For the legacy version use the UseNewtonsoftJson method of TelegramBotBase.Extensions.Serializer.Legacy.NewtonsoftJson</seealso>
    /// </remarks>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <seealso cref="UseNewtonsoftJson"/>
    ILanguageSelectionStage UseJSON();

    /// <summary>
    ///     Using the complex version of .Net JSON, which can serialize all objects.
    ///     Saves in application directory.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Has been changed lately to not use Newtonsoft.Json anymore.
    ///     For the legacy version add the nuget package below and use the <seealso cref="UseNewtonsoftJson"></seealso> method.
    ///     </para>
    ///     <seealso href="https://www.nuget.org/packages/TelegramBotBase.Extensions.Serializer.Legacy.NewtonsoftJson/">For the legacy version use the UseNewtonsoftJson method of TelegramBotBase.Extensions.Serializer.Legacy.NewtonsoftJson</seealso>
    /// </remarks>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <seealso cref="UseNewtonsoftJson"/>
    ILanguageSelectionStage UseJSON(string path);

    /// <summary>
    ///     Use the easy version of .Net JSON, which can serialize basic types, but not generics and others.
    ///     Saves in application directory.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    [Obsolete("Use UseJSON instead.")]
    ILanguageSelectionStage UseSimpleJSON();

    /// <summary>
    ///     Use the easy version of .Net JSON, which can serialize basic types, but not generics and others.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    [Obsolete("Use UseJSON instead.")]
    ILanguageSelectionStage UseSimpleJSON(string path);

    /// <summary>
    ///     Uses the XML serializer for session serialization.
    ///     Saves in application directory.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    ILanguageSelectionStage UseXML();

    /// <summary>
    ///     Uses the XML serializer for session serialization.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    ILanguageSelectionStage UseXML(string path);
}