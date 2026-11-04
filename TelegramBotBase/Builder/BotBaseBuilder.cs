using System;
using System.Collections.Generic;
using System.IO;
using TelegramBotBase.Base;
using TelegramBotBase.Commands;
using TelegramBotBase.Factories;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Interfaces.ExternalActions;
using TelegramBotBase.Localizations;
using TelegramBotBase.MessageLoops;
using TelegramBotBase.RequestDispatchers;
using TelegramBotBase.Sessions;
using TelegramBotBase.States;

namespace TelegramBotBase.Builder;

/// <summary>
///     Configures and creates a <see cref="BotBase" />.
///     Only the API key (<see cref="Create" />) and a start form or form factory are required,
///     every other setting is optional and has a default:
///     <list type="bullet">
///         <item>message loop: <see cref="FormBaseMessageLoop" /></item>
///         <item>networking: direct connection, 60 seconds timeout, pending updates are kept</item>
///         <item>request dispatcher: <see cref="DefaultRequestDispatcher" /> with default settings</item>
///         <item>bot commands: only <c>/start</c></item>
///         <item>serialization: none</item>
///         <item>language: the current <see cref="Default.Language" />, which is English by default</item>
///         <item>threading: single thread</item>
///         <item>session limits: none</item>
///     </list>
///     The order of the calls does not matter, if a setting is configured twice the last call wins.
///     All objects are created in <see cref="Build" />.
/// </summary>
public sealed class BotBaseBuilder
{
    private const string StartCommandDescription = "Starts the bot";

    private readonly string _apiKey;

    private readonly NetworkingOptions _networking = new();

    private List<BotCommandScopeGroup> _botCommandScopes = CreateStartCommandOnly();

    private Func<IMessageLoopFactory> _messageLoop;

    private Func<IFormFactory> _formFactory;

    private Func<MessageClient, IRequestDispatcher> _requestDispatcher;

    private IStateMachine _stateMachine;

    private Localization _language;

    private bool _useThreadPool;

    private int? _workerThreads;

    private int? _ioThreads;

    private SessionLimits _sessionLimits;

    private bool _built;

    private BotBaseBuilder(string apiKey)
    {
        _apiKey = apiKey;
    }

    /// <summary>
    ///     Creates a new builder for a bot with the given API key.
    /// </summary>
    /// <exception cref="ArgumentException">The API key is null or empty.</exception>
    public static BotBaseBuilder Create(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("The API key must not be empty.", nameof(apiKey));
        }

        return new BotBaseBuilder(apiKey);
    }

    #region "Message loop"

    /// <summary>
    ///     Uses the <see cref="FormBaseMessageLoop" /> with the given external action manager.
    ///     Without a message loop call the <see cref="FormBaseMessageLoop" /> is used without an external action manager.
    /// </summary>
    public BotBaseBuilder DefaultMessageLoop(IExternalActionManager managerInstance)
    {
        _messageLoop = () => new FormBaseMessageLoop { ExternalActionManager = managerInstance };
        return this;
    }

    /// <summary>
    ///     Uses a <see cref="MiddlewareBaseMessageLoop" /> configured by the given delegate.
    /// </summary>
    public BotBaseBuilder MiddlewareMessageLoop(
        Func<MiddlewareBaseMessageLoop, MiddlewareBaseMessageLoop> messageLoopConfiguration)
    {
        if (messageLoopConfiguration == null)
        {
            throw new ArgumentNullException(nameof(messageLoopConfiguration));
        }

        _messageLoop = () => messageLoopConfiguration(new MiddlewareBaseMessageLoop());
        return this;
    }

    /// <summary>
    ///     Uses the <see cref="MinimalMessageLoop" />, which only handles the load event of forms.
    /// </summary>
    public BotBaseBuilder MinimalMessageLoop()
    {
        _messageLoop = () => new MinimalMessageLoop();
        return this;
    }

    /// <summary>
    ///     Uses the <see cref="FullMessageLoop" />, which handles all update types.
    /// </summary>
    public BotBaseBuilder FullMessageLoop(IExternalActionManager managerInstance = null)
    {
        _messageLoop = () => new FullMessageLoop { ExternalActionManager = managerInstance };
        return this;
    }

    /// <summary>
    ///     Uses the given message loop instance.
    /// </summary>
    public BotBaseBuilder CustomMessageLoop(IMessageLoopFactory messageLoop)
    {
        if (messageLoop == null)
        {
            throw new ArgumentNullException(nameof(messageLoop));
        }

        _messageLoop = () => messageLoop;
        return this;
    }

    /// <summary>
    ///     Uses a new instance of the given message loop type.
    /// </summary>
    public BotBaseBuilder CustomMessageLoop<T>()
        where T : class, IMessageLoopFactory, new()
    {
        _messageLoop = () => new T();
        return this;
    }

    #endregion

    #region "Start form / form factory (required)"

    /// <summary>
    ///     Uses the given form as start form. It needs a parameterless constructor.
    /// </summary>
    public BotBaseBuilder WithStartForm(Type startFormClass)
    {
        if (startFormClass == null)
        {
            throw new ArgumentNullException(nameof(startFormClass));
        }

        _formFactory = () => new DefaultFormFactory(startFormClass);
        return this;
    }

    /// <summary>
    ///     Uses the given form as start form.
    /// </summary>
    public BotBaseBuilder WithStartForm<T>()
        where T : FormBase, new()
    {
        _formFactory = () => new DefaultFormFactory(typeof(T));
        return this;
    }

    /// <summary>
    ///     Uses the given form as start form and creates all forms via the service provider.
    /// </summary>
    public BotBaseBuilder WithServiceProvider(Type startFormClass, IServiceProvider serviceProvider)
    {
        if (startFormClass == null)
        {
            throw new ArgumentNullException(nameof(startFormClass));
        }

        if (serviceProvider == null)
        {
            throw new ArgumentNullException(nameof(serviceProvider));
        }

        _formFactory = () => new ServiceProviderFormFactory(startFormClass, serviceProvider);
        return this;
    }

    /// <summary>
    ///     Uses the given form as start form and creates all forms via the service provider.
    /// </summary>
    public BotBaseBuilder WithServiceProvider<T>(IServiceProvider serviceProvider)
        where T : FormBase
    {
        if (serviceProvider == null)
        {
            throw new ArgumentNullException(nameof(serviceProvider));
        }

        _formFactory = () => new ServiceProviderFormFactory<T>(serviceProvider);
        return this;
    }

    /// <summary>
    ///     Uses the given form factory to create the start form and all other forms.
    /// </summary>
    public BotBaseBuilder WithFormFactory(IFormFactory factory)
    {
        if (factory == null)
        {
            throw new ArgumentNullException(nameof(factory));
        }

        _formFactory = () => factory;
        return this;
    }

    #endregion

    #region "Networking"

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
    public BotBaseBuilder ConfigureNetworking(Action<NetworkingOptions> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        configure(_networking);
        return this;
    }

    #endregion

    #region "Request dispatcher"

    /// <summary>
    ///     Uses the <see cref="DefaultRequestDispatcher" /> with the given settings.
    ///     Without this call the default dispatcher with default settings is used.
    /// </summary>
    public BotBaseBuilder UseDefaultRequestDispatcher(RequestDispatcherSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        _requestDispatcher = client => new DefaultRequestDispatcher(settings, client);
        return this;
    }

    /// <summary>
    ///     Uses the <see cref="FullRequestDispatcher" />.
    /// </summary>
    public BotBaseBuilder UseFullRequestDispatcher(RequestDispatcherSettings settings = null)
    {
        settings ??= new RequestDispatcherSettings();
        _requestDispatcher = client => new FullRequestDispatcher(settings, client);
        return this;
    }

    /// <summary>
    ///     Uses the <see cref="DirectDispatcher" />, which sends every request immediately.
    /// </summary>
    public BotBaseBuilder UseDirectDispatcher()
    {
        _requestDispatcher = client => new DirectDispatcher(client);
        return this;
    }

    /// <summary>
    ///     Uses a custom dispatcher. The factory receives the final <see cref="MessageClient" /> of the bot.
    /// </summary>
    public BotBaseBuilder UseCustomRequestDispatcher(Func<MessageClient, IRequestDispatcher> factory)
    {
        _requestDispatcher = factory ?? throw new ArgumentNullException(nameof(factory));
        return this;
    }

    /// <summary>
    ///     Uses the given dispatcher instance.
    /// </summary>
    public BotBaseBuilder UseCustomRequestDispatcher(IRequestDispatcher dispatcher)
    {
        if (dispatcher == null)
        {
            throw new ArgumentNullException(nameof(dispatcher));
        }

        _requestDispatcher = _ => dispatcher;
        return this;
    }

    #endregion

    #region "Bot commands"

    /// <summary>
    ///     Registers no bot commands. By default only <c>/start</c> is registered.
    /// </summary>
    public BotBaseBuilder NoCommands()
    {
        _botCommandScopes = new List<BotCommandScopeGroup>();
        return this;
    }

    /// <summary>
    ///     Registers <c>/start</c>, <c>/help</c> and <c>/settings</c>.
    /// </summary>
    public BotBaseBuilder DefaultCommands()
    {
        _botCommandScopes = CreateStartCommandOnly();
        _botCommandScopes.Help("Should show you some help");
        _botCommandScopes.Settings("Should show you some settings");
        return this;
    }

    /// <summary>
    ///     Replaces the bot commands with the ones added by the delegate. The list passed to it is empty,
    ///     so add <c>/start</c> again if you need it.
    /// </summary>
    public BotBaseBuilder CustomCommands(Action<List<BotCommandScopeGroup>> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        _botCommandScopes = new List<BotCommandScopeGroup>();
        action(_botCommandScopes);
        return this;
    }

    private static List<BotCommandScopeGroup> CreateStartCommandOnly()
    {
        var scopes = new List<BotCommandScopeGroup>();
        scopes.Start(StartCommandDescription);
        return scopes;
    }

    #endregion

    #region "Serialization"

    /// <summary>
    ///     Saves and restores the sessions with the given state machine. By default nothing is serialized.
    /// </summary>
    public BotBaseBuilder UseSerialization(IStateMachine machine)
    {
        _stateMachine = machine ?? throw new ArgumentNullException(nameof(machine));
        return this;
    }

    /// <summary>
    ///     Uses the application runtime path to load and write a states.json file.
    /// </summary>
    /// <seealso href="https://www.nuget.org/packages/TelegramBotBase.Extensions.Serializer.Legacy.NewtonsoftJson/">
    ///     For the legacy version use the UseNewtonsoftJson method of
    ///     TelegramBotBase.Extensions.Serializer.Legacy.NewtonsoftJson
    /// </seealso>
    public BotBaseBuilder UseJSON(Type fallbackForm = null)
    {
        return UseJSON(Path.Combine(Directory.GetCurrentDirectory(), "states.json"), fallbackForm);
    }

    /// <summary>
    ///     Uses the given path to load and write a states.json file.
    /// </summary>
    /// <seealso href="https://www.nuget.org/packages/TelegramBotBase.Extensions.Serializer.Legacy.NewtonsoftJson/">
    ///     For the legacy version use the UseNewtonsoftJson method of
    ///     TelegramBotBase.Extensions.Serializer.Legacy.NewtonsoftJson
    /// </seealso>
    public BotBaseBuilder UseJSON(string path, Type fallbackForm = null)
    {
        return UseSerialization(new JsonStateMachine(path, fallbackForm));
    }

    /// <summary>
    ///     Uses the application runtime path to load and write a states.json file.
    /// </summary>
    [Obsolete("Use UseJSON instead.")]
    public BotBaseBuilder UseSimpleJSON(Type fallbackForm = null)
    {
        return UseSimpleJSON(Path.Combine(Directory.GetCurrentDirectory(), "states.json"), fallbackForm);
    }

    /// <summary>
    ///     Uses the given path to load and write a states.json file.
    /// </summary>
    [Obsolete("Use UseJSON instead.")]
    public BotBaseBuilder UseSimpleJSON(string path, Type fallbackForm = null)
    {
        return UseSerialization(new SimpleJsonStateMachine(path, fallbackForm));
    }

    /// <summary>
    ///     Uses the application runtime path to load and write a states.xml file.
    /// </summary>
    public BotBaseBuilder UseXML(Type fallbackForm = null)
    {
        return UseXML(Path.Combine(Directory.GetCurrentDirectory(), "states.xml"), fallbackForm);
    }

    /// <summary>
    ///     Uses the given path to load and write a states.xml file.
    /// </summary>
    public BotBaseBuilder UseXML(string path, Type fallbackForm = null)
    {
        return UseSerialization(new XmlStateMachine(path, fallbackForm));
    }

    #endregion

    #region "Language"

    /// <summary>
    ///     Sets <see cref="Default.Language" /> to English when the bot is built.
    /// </summary>
    public BotBaseBuilder UseEnglish()
    {
        return UseLanguage(new English());
    }

    /// <summary>
    ///     Sets <see cref="Default.Language" /> to German when the bot is built.
    /// </summary>
    public BotBaseBuilder UseGerman()
    {
        return UseLanguage(new German());
    }

    /// <summary>
    ///     Sets <see cref="Default.Language" /> to Persian when the bot is built.
    /// </summary>
    public BotBaseBuilder UsePersian()
    {
        return UseLanguage(new Persian());
    }

    /// <summary>
    ///     Sets <see cref="Default.Language" /> to Russian when the bot is built.
    /// </summary>
    public BotBaseBuilder UseRussian()
    {
        return UseLanguage(new Russian());
    }

    /// <summary>
    ///     Sets <see cref="Default.Language" /> to Ukrainian when the bot is built.
    /// </summary>
    public BotBaseBuilder UseUkrainian()
    {
        return UseLanguage(new Ukrainian());
    }

    /// <summary>
    ///     Sets <see cref="Default.Language" /> to Belarusian when the bot is built.
    /// </summary>
    public BotBaseBuilder UseBelarusian()
    {
        return UseLanguage(new Belarusian());
    }

    /// <summary>
    ///     Sets <see cref="Default.Language" /> to the given localization when the bot is built.
    ///     Without a language call the bot uses the current <see cref="Default.Language" />.
    /// </summary>
    public BotBaseBuilder UseLanguage(Localization language)
    {
        _language = language ?? throw new ArgumentNullException(nameof(language));
        return this;
    }

    #endregion

    #region "Threading"

    /// <summary>
    ///     Handles every update on the thread pool instead of a single thread.
    /// </summary>
    public BotBaseBuilder UseThreadPool()
    {
        _useThreadPool = true;
        _workerThreads = null;
        _ioThreads = null;
        return this;
    }

    /// <summary>
    ///     Handles every update on the thread pool and sets its maximum number of threads on start.
    /// </summary>
    public BotBaseBuilder UseThreadPool(int workerThreads, int ioThreads)
    {
        _useThreadPool = true;
        _workerThreads = workerThreads;
        _ioThreads = ioThreads;
        return this;
    }

    #endregion

    #region "Session limits"

    /// <summary>
    ///     Uses the recommended session limits, see <see cref="SessionLimits.CreateDefault" />.
    ///     By default there are no limits.
    /// </summary>
    public BotBaseBuilder WithDefaultSessionLimits()
    {
        return WithSessionLimits(SessionLimits.CreateDefault());
    }

    /// <summary>
    ///     Uses session limits configured by the delegate.
    /// </summary>
    public BotBaseBuilder WithSessionLimits(Action<SessionLimits> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var limits = new SessionLimits();
        configure(limits);

        return WithSessionLimits(limits);
    }

    /// <summary>
    ///     Uses a copy of the given session limits.
    /// </summary>
    public BotBaseBuilder WithSessionLimits(SessionLimits limits)
    {
        if (limits == null)
        {
            throw new ArgumentNullException(nameof(limits));
        }

        _sessionLimits = limits.Clone();
        return this;
    }

    #endregion

    /// <summary>
    ///     Validates the configuration and creates the bot. A builder can only build one bot.
    /// </summary>
    /// <exception cref="BotBaseBuilderException">The configuration is invalid.</exception>
    /// <exception cref="InvalidOperationException">The builder has already built a bot.</exception>
    public BotBase Build()
    {
        if (_built)
        {
            throw new InvalidOperationException("This builder has already built a bot. Create a new builder for another bot.");
        }

        var errors = new List<string>();

        var formFactory = Create(_formFactory, errors, "Start form",
                                 "No start form is configured. Call WithStartForm, WithServiceProvider or WithFormFactory.");

        var messageLoop = Create(_messageLoop ?? (() => new FormBaseMessageLoop()), errors, "Message loop",
                                 "The message loop configuration returned null.");

        _networking.Validate(errors);

        if (_useThreadPool && _workerThreads != null && (_workerThreads < 1 || _ioThreads < 1))
        {
            errors.Add($"Threading: the thread pool needs at least 1 worker and 1 IO thread, but got {_workerThreads} and {_ioThreads}.");
        }

        try
        {
            _sessionLimits?.Validate();
        }
        catch (ArgumentException ex)
        {
            errors.Add("Session limits: " + ex.Message);
        }

        MessageClient client = null;
        if (errors.Count == 0)
        {
            client = Create(() => _networking.CreateClient(_apiKey, _useThreadPool), errors, "API key",
                            "The message client could not be created.");
        }

        IRequestDispatcher requestDispatcher = null;
        if (client != null)
        {
            if (client is ThreadPoolMessageClient threadPoolClient)
            {
                threadPoolClient.ThreadPool_WorkerThreads = _workerThreads;
                threadPoolClient.ThreadPool_IOThreads = _ioThreads;
            }

            client.AllowedUpdates = messageLoop.ConfigureUpdateTypes();

            // Created from the final client, so the dispatcher and the bot always share the same one.
            var dispatcherFactory = _requestDispatcher ??
                                    (c => new DefaultRequestDispatcher(new RequestDispatcherSettings(), c));

            requestDispatcher = Create(() => dispatcherFactory(client), errors, "Request dispatcher",
                                       "The custom request dispatcher factory returned null.");
        }

        if (errors.Count > 0)
        {
            throw new BotBaseBuilderException(errors);
        }

        if (_language != null)
        {
            Default.Language = _language;
        }

        _built = true;

        return new BotBase(_apiKey, client)
        {
            FormFactory = formFactory,
            BotCommandScopes = _botCommandScopes,
            StateMachine = _stateMachine,
            MessageLoopFactory = messageLoop,
            RequestDispatcher = requestDispatcher,
            SessionLimits = _sessionLimits
        };
    }

    /// <summary>
    ///     Runs the factory and records a missing result or an argument error instead of throwing.
    /// </summary>
    private static T Create<T>(Func<T> factory, ICollection<string> errors, string area, string missingMessage)
        where T : class
    {
        if (factory == null)
        {
            errors.Add($"{area}: {missingMessage}");
            return null;
        }

        try
        {
            var result = factory();
            if (result == null)
            {
                errors.Add($"{area}: {missingMessage}");
            }

            return result;
        }
        catch (ArgumentException ex)
        {
            errors.Add($"{area}: {ex.Message}");
            return null;
        }
    }
}
