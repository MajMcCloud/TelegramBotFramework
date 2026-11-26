using System;
using System.Collections.Generic;
using Telegram.Bot.Types;
using TelegramBotBase.Base;
using TelegramBotBase.Commands;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Interfaces.ExternalActions;
using TelegramBotBase.Localizations;
using TelegramBotBase.MessageLoops;
using TelegramBotBase.RequestDispatchers;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Builder;

/// <summary>
///     Configures and creates a <see cref="BotBase" />.
///     Pass the API key to <see cref="BotBaseBuilder.Create" /> and configure a start form or form factory.
///     All other settings are optional. The builder uses these defaults.
///     <list type="bullet">
///         <item><see cref="FormBaseMessageLoop" /></item>
///         <item>A direct connection with a 60-second timeout that keeps pending updates</item>
///         <item><see cref="DefaultRequestDispatcher" /> with default settings</item>
///         <item>Only the <c>/start</c> command</item>
///         <item>No session serialization</item>
///         <item>The current <see cref="Default.Language" />, which is English by default</item>
///         <item>Single-threaded update handling</item>
///         <item>No session limits</item>
///     </list>
///     Configuration methods return the builder and can be called in any order.
///     If a setting is configured twice, the last call wins.
/// </summary>
public interface IBotBaseBuilder
{
    /// <summary>
    ///     Uses the <see cref="FormBaseMessageLoop" /> with the given external action manager.
    ///     Without a message loop call the <see cref="FormBaseMessageLoop" /> is used without an external action manager.
    /// </summary>
    IBotBaseBuilder DefaultMessageLoop(IExternalActionManager managerInstance);

    /// <summary>
    ///     Uses a <see cref="MiddlewareBaseMessageLoop" /> configured by the given delegate.
    /// </summary>
    IBotBaseBuilder MiddlewareMessageLoop(
        Func<MiddlewareBaseMessageLoop, MiddlewareBaseMessageLoop> messageLoopConfiguration);

    /// <summary>
    ///     Uses the <see cref="MessageLoops.MinimalMessageLoop" />, which only handles the load event of forms.
    /// </summary>
    IBotBaseBuilder MinimalMessageLoop();

    /// <summary>
    ///     Uses the <see cref="MessageLoops.FullMessageLoop" />, which handles all update types.
    /// </summary>
    IBotBaseBuilder FullMessageLoop(IExternalActionManager managerInstance = null);

    /// <summary>
    ///     Uses the given message loop instance.
    /// </summary>
    IBotBaseBuilder CustomMessageLoop(IMessageLoopFactory messageLoop);

    /// <summary>
    ///     Uses a new instance of the given message loop type.
    /// </summary>
    IBotBaseBuilder CustomMessageLoop<T>()
        where T : class, IMessageLoopFactory, new();

    /// <summary>
    ///     Uses the given form as start form. It needs a parameterless constructor.
    /// </summary>
    IBotBaseBuilder WithStartForm(Type startFormClass);

    /// <summary>
    ///     Uses the given form as start form.
    /// </summary>
    IBotBaseBuilder WithStartForm<T>()
        where T : FormBase, new();

    /// <summary>
    ///     Uses the given form as start form and creates all forms via the service provider.
    /// </summary>
    IBotBaseBuilder WithServiceProvider(Type startFormClass, IServiceProvider serviceProvider);

    /// <summary>
    ///     Uses the given form as start form and creates all forms via the service provider.
    /// </summary>
    IBotBaseBuilder WithServiceProvider<T>(IServiceProvider serviceProvider)
        where T : FormBase;

    /// <summary>
    ///     Uses the given form factory to create the start form and all other forms.
    /// </summary>
    IBotBaseBuilder WithFormFactory(IFormFactory factory);

    /// <summary>
    ///     Configures proxy, timeout and pending updates. See <see cref="NetworkingOptions" /> for the defaults.
    /// </summary>
    /// <example>
    ///     <code>
    /// builder.ConfigureNetworking(n =>
    /// {
    ///     n.UseProxy("http://127.0.0.1:8080");
    ///     n.Timeout = TimeSpan.FromSeconds(30);
    ///     n.DropPendingUpdates = true;
    /// });
    /// </code>
    /// </example>
    IBotBaseBuilder ConfigureNetworking(Action<NetworkingOptions> configure);

    /// <summary>
    ///     Uses the <see cref="DefaultRequestDispatcher" /> with the given settings.
    ///     Without this call the default dispatcher with default settings is used.
    /// </summary>
    IBotBaseBuilder UseDefaultRequestDispatcher(RequestDispatcherSettings settings);

    /// <summary>
    ///     Uses the <see cref="FullRequestDispatcher" />.
    /// </summary>
    IBotBaseBuilder UseFullRequestDispatcher(RequestDispatcherSettings settings = null);

    /// <summary>
    ///     Uses the <see cref="DirectDispatcher" />, which sends every request immediately.
    /// </summary>
    IBotBaseBuilder UseDirectDispatcher();

    /// <summary>
    ///     Uses a custom dispatcher. The factory receives the final <see cref="MessageClient" /> of the bot.
    /// </summary>
    IBotBaseBuilder UseCustomRequestDispatcher(Func<MessageClient, IRequestDispatcher> factory);

    /// <summary>
    ///     Uses the given dispatcher instance.
    /// </summary>
    IBotBaseBuilder UseCustomRequestDispatcher(IRequestDispatcher dispatcher);

    /// <summary>
    ///     Registers no bot commands. By default only <c>/start</c> is registered.
    /// </summary>
    IBotBaseBuilder NoCommands();

    /// <summary>
    ///     Registers <c>/start</c>, <c>/help</c> and <c>/settings</c>.
    /// </summary>
    IBotBaseBuilder DefaultCommands();

    /// <summary>
    ///     Replaces the bot commands with the ones added by the delegate. The list passed to it is empty,
    ///     so add <c>/start</c> again if you need it.
    /// </summary>
    IBotBaseBuilder CustomCommands(Action<List<BotCommandScopeGroup>> action);

    /// <summary>
    ///     Saves and restores the sessions with the given state machine. By default nothing is serialized.
    /// </summary>
    IBotBaseBuilder UseSerialization(IStateMachine machine);

    /// <summary>
    ///     Uses the application runtime path to load and write a states.json file.
    /// </summary>
    /// <seealso href="https://www.nuget.org/packages/TelegramBotBase.Extensions.Serializer.Legacy.NewtonsoftJson/">
    ///     For the legacy version use the UseNewtonsoftJson method of
    ///     TelegramBotBase.Extensions.Serializer.Legacy.NewtonsoftJson
    /// </seealso>
    IBotBaseBuilder UseJSON(Type fallbackForm = null);

    /// <summary>
    ///     Uses the given path to load and write a states.json file.
    /// </summary>
    /// <seealso href="https://www.nuget.org/packages/TelegramBotBase.Extensions.Serializer.Legacy.NewtonsoftJson/">
    ///     For the legacy version use the UseNewtonsoftJson method of
    ///     TelegramBotBase.Extensions.Serializer.Legacy.NewtonsoftJson
    /// </seealso>
    IBotBaseBuilder UseJSON(string path, Type fallbackForm = null);

    /// <summary>
    ///     Uses the application runtime path to load and write a states.json file.
    /// </summary>
    [Obsolete("Use UseJSON instead.")]
    IBotBaseBuilder UseSimpleJSON(Type fallbackForm = null);

    /// <summary>
    ///     Uses the given path to load and write a states.json file.
    /// </summary>
    [Obsolete("Use UseJSON instead.")]
    IBotBaseBuilder UseSimpleJSON(string path, Type fallbackForm = null);

    /// <summary>
    ///     Uses the application runtime path to load and write a states.xml file.
    /// </summary>
    IBotBaseBuilder UseXML(Type fallbackForm = null);

    /// <summary>
    ///     Uses the given path to load and write a states.xml file.
    /// </summary>
    IBotBaseBuilder UseXML(string path, Type fallbackForm = null);

    /// <summary>
    ///     Sets <see cref="Default.Language" /> to English when the bot is built.
    /// </summary>
    IBotBaseBuilder UseEnglish();

    /// <summary>
    ///     Sets <see cref="Default.Language" /> to German when the bot is built.
    /// </summary>
    IBotBaseBuilder UseGerman();

    /// <summary>
    ///     Sets <see cref="Default.Language" /> to Persian when the bot is built.
    /// </summary>
    IBotBaseBuilder UsePersian();

    /// <summary>
    ///     Sets <see cref="Default.Language" /> to Russian when the bot is built.
    /// </summary>
    IBotBaseBuilder UseRussian();

    /// <summary>
    ///     Sets <see cref="Default.Language" /> to Ukrainian when the bot is built.
    /// </summary>
    IBotBaseBuilder UseUkrainian();

    /// <summary>
    ///     Sets <see cref="Default.Language" /> to Belarusian when the bot is built.
    /// </summary>
    IBotBaseBuilder UseBelarusian();

    /// <summary>
    ///     Sets <see cref="Default.Language" /> to the given localization when the bot is built.
    ///     Without a language call the bot uses the current <see cref="Default.Language" />.
    /// </summary>
    IBotBaseBuilder UseLanguage(Localization language);

    /// <summary>
    ///     Receives updates via a Telegram webhook. The HTTP host can verify the secret with
    ///     <see cref="BotBase.IsValidWebhookSecret" /> and pass updates to <see cref="BotBase.HandleUpdateAsync" />.
    /// </summary>
    /// <param name="url">The public HTTPS URL that receives Telegram updates.</param>
    /// <param name="secretToken">The value Telegram sends in the X-Telegram-Bot-Api-Secret-Token header.</param>
    /// <param name="maxConnections">The optional maximum number of simultaneous delivery connections (1–100).</param>
    /// <param name="deleteOnStop">Whether to remove the webhook from Telegram when the bot stops.</param>
    /// <param name="certificateFactory">Optional factory for a fresh public certificate stream on each start.</param>
    IBotBaseBuilder WithWebhook(string url, string secretToken, int? maxConnections = null,
                                bool deleteOnStop = false, Func<InputFileStream> certificateFactory = null);

    /// <summary>
    ///     Handles each polled update on the thread pool instead of a single thread.
    /// </summary>
    IBotBaseBuilder UseThreadPool();

    /// <summary>
    ///     Handles each polled update on the thread pool and sets its maximum number of threads on start.
    /// </summary>
    IBotBaseBuilder UseThreadPool(int workerThreads, int ioThreads);

    /// <summary>
    ///     Uses the recommended session limits, see <see cref="SessionLimits.CreateDefault" />.
    ///     By default there are no limits.
    /// </summary>
    IBotBaseBuilder WithDefaultSessionLimits();

    /// <summary>
    ///     Uses session limits configured by the delegate.
    /// </summary>
    IBotBaseBuilder WithSessionLimits(Action<SessionLimits> configure);

    /// <summary>
    ///     Uses a copy of the given session limits.
    /// </summary>
    IBotBaseBuilder WithSessionLimits(SessionLimits limits);

    /// <summary>
    ///     Validates the configuration and creates the bot. A builder can only build one bot.
    /// </summary>
    /// <exception cref="BotBaseBuilderException">The configuration is invalid.</exception>
    /// <exception cref="InvalidOperationException">The builder has already built a bot.</exception>
    BotBase Build();
}
