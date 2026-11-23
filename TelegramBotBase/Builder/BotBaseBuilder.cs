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

/// <inheritdoc cref="IBotBaseBuilder" />
public sealed class BotBaseBuilder : IBotBaseBuilder
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
    public static IBotBaseBuilder Create(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("The API key must not be empty.", nameof(apiKey));
        }

        return new BotBaseBuilder(apiKey);
    }

    #region "Message loop"

    /// <inheritdoc />
    public IBotBaseBuilder DefaultMessageLoop(IExternalActionManager managerInstance)
    {
        _messageLoop = () => new FormBaseMessageLoop { ExternalActionManager = managerInstance };
        return this;
    }

    /// <inheritdoc />
    public IBotBaseBuilder MiddlewareMessageLoop(
        Func<MiddlewareBaseMessageLoop, MiddlewareBaseMessageLoop> messageLoopConfiguration)
    {
        if (messageLoopConfiguration == null)
        {
            throw new ArgumentNullException(nameof(messageLoopConfiguration));
        }

        _messageLoop = () => messageLoopConfiguration(new MiddlewareBaseMessageLoop());
        return this;
    }

    /// <inheritdoc />
    public IBotBaseBuilder MinimalMessageLoop()
    {
        _messageLoop = () => new MinimalMessageLoop();
        return this;
    }

    /// <inheritdoc />
    public IBotBaseBuilder FullMessageLoop(IExternalActionManager managerInstance = null)
    {
        _messageLoop = () => new FullMessageLoop { ExternalActionManager = managerInstance };
        return this;
    }

    /// <inheritdoc />
    public IBotBaseBuilder CustomMessageLoop(IMessageLoopFactory messageLoop)
    {
        if (messageLoop == null)
        {
            throw new ArgumentNullException(nameof(messageLoop));
        }

        _messageLoop = () => messageLoop;
        return this;
    }

    /// <inheritdoc />
    public IBotBaseBuilder CustomMessageLoop<T>()
        where T : class, IMessageLoopFactory, new()
    {
        _messageLoop = () => new T();
        return this;
    }

    #endregion

    #region "Start form / form factory (required)"

    /// <inheritdoc />
    public IBotBaseBuilder WithStartForm(Type startFormClass)
    {
        if (startFormClass == null)
        {
            throw new ArgumentNullException(nameof(startFormClass));
        }

        _formFactory = () => new DefaultFormFactory(startFormClass);
        return this;
    }

    /// <inheritdoc />
    public IBotBaseBuilder WithStartForm<T>()
        where T : FormBase, new()
    {
        _formFactory = () => new DefaultFormFactory(typeof(T));
        return this;
    }

    /// <inheritdoc />
    public IBotBaseBuilder WithServiceProvider(Type startFormClass, IServiceProvider serviceProvider)
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

    /// <inheritdoc />
    public IBotBaseBuilder WithServiceProvider<T>(IServiceProvider serviceProvider)
        where T : FormBase
    {
        if (serviceProvider == null)
        {
            throw new ArgumentNullException(nameof(serviceProvider));
        }

        _formFactory = () => new ServiceProviderFormFactory<T>(serviceProvider);
        return this;
    }

    /// <inheritdoc />
    public IBotBaseBuilder WithFormFactory(IFormFactory factory)
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

    /// <inheritdoc />
    public IBotBaseBuilder ConfigureNetworking(Action<NetworkingOptions> configure)
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

    /// <inheritdoc />
    public IBotBaseBuilder UseDefaultRequestDispatcher(RequestDispatcherSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        _requestDispatcher = client => new DefaultRequestDispatcher(settings, client);
        return this;
    }

    /// <inheritdoc />
    public IBotBaseBuilder UseFullRequestDispatcher(RequestDispatcherSettings settings = null)
    {
        settings ??= new RequestDispatcherSettings();
        _requestDispatcher = client => new FullRequestDispatcher(settings, client);
        return this;
    }

    /// <inheritdoc />
    public IBotBaseBuilder UseDirectDispatcher()
    {
        _requestDispatcher = client => new DirectDispatcher(client);
        return this;
    }

    /// <inheritdoc />
    public IBotBaseBuilder UseCustomRequestDispatcher(Func<MessageClient, IRequestDispatcher> factory)
    {
        _requestDispatcher = factory ?? throw new ArgumentNullException(nameof(factory));
        return this;
    }

    /// <inheritdoc />
    public IBotBaseBuilder UseCustomRequestDispatcher(IRequestDispatcher dispatcher)
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

    /// <inheritdoc />
    public IBotBaseBuilder NoCommands()
    {
        _botCommandScopes = new List<BotCommandScopeGroup>();
        return this;
    }

    /// <inheritdoc />
    public IBotBaseBuilder DefaultCommands()
    {
        _botCommandScopes = CreateStartCommandOnly();
        _botCommandScopes.Help("Should show you some help");
        _botCommandScopes.Settings("Should show you some settings");
        return this;
    }

    /// <inheritdoc />
    public IBotBaseBuilder CustomCommands(Action<List<BotCommandScopeGroup>> action)
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

    /// <inheritdoc />
    public IBotBaseBuilder UseSerialization(IStateMachine machine)
    {
        _stateMachine = machine ?? throw new ArgumentNullException(nameof(machine));
        return this;
    }

    /// <inheritdoc />
    public IBotBaseBuilder UseJSON(Type fallbackForm = null)
    {
        return UseJSON(Path.Combine(Directory.GetCurrentDirectory(), "states.json"), fallbackForm);
    }

    /// <inheritdoc />
    public IBotBaseBuilder UseJSON(string path, Type fallbackForm = null)
    {
        return UseSerialization(new JsonStateMachine(path, fallbackForm));
    }

    /// <inheritdoc />
    [Obsolete("Use UseJSON instead.")]
    public IBotBaseBuilder UseSimpleJSON(Type fallbackForm = null)
    {
        return UseSimpleJSON(Path.Combine(Directory.GetCurrentDirectory(), "states.json"), fallbackForm);
    }

    /// <inheritdoc />
    [Obsolete("Use UseJSON instead.")]
    public IBotBaseBuilder UseSimpleJSON(string path, Type fallbackForm = null)
    {
        return UseSerialization(new SimpleJsonStateMachine(path, fallbackForm));
    }

    /// <inheritdoc />
    public IBotBaseBuilder UseXML(Type fallbackForm = null)
    {
        return UseXML(Path.Combine(Directory.GetCurrentDirectory(), "states.xml"), fallbackForm);
    }

    /// <inheritdoc />
    public IBotBaseBuilder UseXML(string path, Type fallbackForm = null)
    {
        return UseSerialization(new XmlStateMachine(path, fallbackForm));
    }

    #endregion

    #region "Language"

    /// <inheritdoc />
    public IBotBaseBuilder UseEnglish()
    {
        return UseLanguage(new English());
    }

    /// <inheritdoc />
    public IBotBaseBuilder UseGerman()
    {
        return UseLanguage(new German());
    }

    /// <inheritdoc />
    public IBotBaseBuilder UsePersian()
    {
        return UseLanguage(new Persian());
    }

    /// <inheritdoc />
    public IBotBaseBuilder UseRussian()
    {
        return UseLanguage(new Russian());
    }

    /// <inheritdoc />
    public IBotBaseBuilder UseUkrainian()
    {
        return UseLanguage(new Ukrainian());
    }

    /// <inheritdoc />
    public IBotBaseBuilder UseBelarusian()
    {
        return UseLanguage(new Belarusian());
    }

    /// <inheritdoc />
    public IBotBaseBuilder UseLanguage(Localization language)
    {
        _language = language ?? throw new ArgumentNullException(nameof(language));
        return this;
    }

    #endregion

    #region "Threading"

    /// <inheritdoc />
    public IBotBaseBuilder UseThreadPool()
    {
        _useThreadPool = true;
        _workerThreads = null;
        _ioThreads = null;
        return this;
    }

    /// <inheritdoc />
    public IBotBaseBuilder UseThreadPool(int workerThreads, int ioThreads)
    {
        _useThreadPool = true;
        _workerThreads = workerThreads;
        _ioThreads = ioThreads;
        return this;
    }

    #endregion

    #region "Session limits"

    /// <inheritdoc />
    public IBotBaseBuilder WithDefaultSessionLimits()
    {
        return WithSessionLimits(SessionLimits.CreateDefault());
    }

    /// <inheritdoc />
    public IBotBaseBuilder WithSessionLimits(Action<SessionLimits> configure)
    {
        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        var limits = new SessionLimits();
        configure(limits);

        return WithSessionLimits(limits);
    }

    /// <inheritdoc />
    public IBotBaseBuilder WithSessionLimits(SessionLimits limits)
    {
        if (limits == null)
        {
            throw new ArgumentNullException(nameof(limits));
        }

        _sessionLimits = limits.Clone();
        return this;
    }

    #endregion

    /// <inheritdoc />
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
