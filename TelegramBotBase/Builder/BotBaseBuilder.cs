﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotBase.Base;
using TelegramBotBase.Builder.Interfaces;
using TelegramBotBase.Commands;
using TelegramBotBase.Factories;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Localizations;
using TelegramBotBase.MessageLoops;
using TelegramBotBase.States;

namespace TelegramBotBase.Builder;

public class BotBaseBuilder : IAPIKeySelectionStage, IMessageLoopSelectionStage, IStartFormSelectionStage,
                              IBuildingStage, INetworkingSelectionStage, IBotCommandsStage, ISessionSerializationStage,
                              ILanguageSelectionStage
{
    private string _apiKey;

    private MessageClient _client;

    private IStartFormFactory _factory;

    private IMessageLoopFactory _messageLoopFactory;

    private IStateMachine _stateMachine;

    private BotBaseBuilder()
    {
    }

    /// <summary>
    ///     Contains different Botcommands for different areas.
    /// </summary>
    private Dictionary<BotCommandScope, List<BotCommand>> BotCommandScopes { get; } = new();

    /// <summary>
    /// Creates a full BotBase instance with all parameters previously set.
    /// </summary>
    /// <returns></returns>
    public BotBase Build()
    {
        var bot = new BotBase(_apiKey, _client)
        {
            StartFormFactory = _factory,
            BotCommandScopes = BotCommandScopes,
            StateMachine = _stateMachine,
            MessageLoopFactory = _messageLoopFactory
        };

        bot.MessageLoopFactory.UnhandledCall += bot.MessageLoopFactory_UnhandledCall;

        return bot;
    }

    public static IAPIKeySelectionStage Create()
    {
        return new BotBaseBuilder();
    }

    #region "Step 1 (Basic Stuff)"

    public IMessageLoopSelectionStage WithAPIKey(string apiKey)
    {
        _apiKey = apiKey;
        return this;
    }


    public IBuildingStage QuickStart(string apiKey, Type startForm)
    {
        _apiKey = apiKey;
        _factory = new DefaultStartFormFactory(startForm);

        DefaultMessageLoop();

        NoProxy();

        OnlyStart();

        NoSerialization();

        DefaultLanguage();

        return this;
    }


    public IBuildingStage QuickStart<T>(string apiKey)
        where T : FormBase
    {
        _apiKey = apiKey;
        _factory = new DefaultStartFormFactory(typeof(T));

        DefaultMessageLoop();

        NoProxy();

        OnlyStart();

        NoSerialization();

        DefaultLanguage();

        return this;
    }

    public IBuildingStage QuickStart(string apiKey, IStartFormFactory startFormFactory)
    {
        _apiKey = apiKey;
        _factory = startFormFactory;

        DefaultMessageLoop();

        NoProxy();

        OnlyStart();

        NoSerialization();

        DefaultLanguage();

        return this;
    }

    #endregion


    #region "Step 2 (Message Loop)"

    public IStartFormSelectionStage DefaultMessageLoop()
    {
        _messageLoopFactory = new FormBaseMessageLoop();

        return this;
    }

    public IStartFormSelectionStage MiddlewareMessageLoop(Func<MiddlewareBaseMessageLoop, MiddlewareBaseMessageLoop> messageLoopConfiguration)
    {
        _messageLoopFactory = messageLoopConfiguration(new MiddlewareBaseMessageLoop());

        return this;
    }

    public IStartFormSelectionStage MinimalMessageLoop()
    {
        _messageLoopFactory = new MinimalMessageLoop();

        return this;
    }


    public IStartFormSelectionStage CustomMessageLoop(IMessageLoopFactory messageLoopClass)
    {
        _messageLoopFactory = messageLoopClass;

        return this;
    }

    public IStartFormSelectionStage CustomMessageLoop<T>()
        where T : class, new()
    {
        _messageLoopFactory =
            typeof(T).GetConstructor(new Type[] { })?.Invoke(new object[] { }) as IMessageLoopFactory;

        return this;
    }

    #endregion


    #region "Step 3 (Start Form/Factory)"

    public INetworkingSelectionStage WithStartForm(Type startFormClass)
    {
        _factory = new DefaultStartFormFactory(startFormClass);
        return this;
    }

    public INetworkingSelectionStage WithStartForm<T>()
        where T : FormBase, new()
    {
        _factory = new DefaultStartFormFactory(typeof(T));
        return this;
    }

    public INetworkingSelectionStage WithServiceProvider(Type startFormClass, IServiceProvider serviceProvider)
    {
        _factory = new ServiceProviderStartFormFactory(startFormClass, serviceProvider);
        return this;
    }

    public INetworkingSelectionStage WithServiceProvider<T>(IServiceProvider serviceProvider)
        where T : FormBase
    {
        _factory = new ServiceProviderStartFormFactory<T>(serviceProvider);
        return this;
    }

    public INetworkingSelectionStage WithStartFormFactory(IStartFormFactory factory)
    {
        _factory = factory;
        return this;
    }

    #endregion


    #region "Step 4 (Network Settings)"

    public IBotCommandsStage WithProxy(string proxyAddress, bool throwPendingUpdates = false)
    {
        var url = new Uri(proxyAddress);
        _client = new MessageClient(_apiKey, url)
        {
            TelegramClient =
            {
                Timeout = new TimeSpan(0, 1, 0)
            },
        };
        _client.ThrowPendingUpdates = throwPendingUpdates;
        return this;
    }


    public IBotCommandsStage NoProxy(bool throwPendingUpdates = false)
    {
        _client = new MessageClient(_apiKey)
        {
            TelegramClient =
            {
                Timeout = new TimeSpan(0, 1, 0)
            }
        };
        _client.ThrowPendingUpdates = throwPendingUpdates;
        return this;
    }


    public IBotCommandsStage WithBotClient(TelegramBotClient tgclient, bool throwPendingUpdates = false)
    {
        _client = new MessageClient(_apiKey, tgclient)
        {
            TelegramClient =
            {
                Timeout = new TimeSpan(0, 1, 0)
            }
        };
        _client.ThrowPendingUpdates = throwPendingUpdates;
        return this;
    }


    public IBotCommandsStage WithHostAndPort(string proxyHost, int proxyPort, bool throwPendingUpdates = false)
    {
        _client = new MessageClient(_apiKey, proxyHost, proxyPort)
        {
            TelegramClient =
            {
                Timeout = new TimeSpan(0, 1, 0)
            }
        };
        _client.ThrowPendingUpdates = throwPendingUpdates;
        return this;
    }

    public IBotCommandsStage WithHttpClient(HttpClient tgclient, bool throwPendingUpdates = false)
    {
        _client = new MessageClient(_apiKey, tgclient)
        {
            TelegramClient =
            {
                Timeout = new TimeSpan(0, 1, 0)
            }
        };
        _client.ThrowPendingUpdates = throwPendingUpdates;
        return this;
    }

    #endregion


    #region "Step 5 (Bot Commands)"

    public ISessionSerializationStage NoCommands()
    {
        return this;
    }

    public ISessionSerializationStage OnlyStart()
    {
        BotCommandScopes.Start("Starts the bot");

        return this;
    }

    public ISessionSerializationStage DefaultCommands()
    {
        BotCommandScopes.Start("Starts the bot");
        BotCommandScopes.Help("Should show you some help");
        BotCommandScopes.Settings("Should show you some settings");
        return this;
    }

    public ISessionSerializationStage CustomCommands(Action<Dictionary<BotCommandScope, List<BotCommand>>> action)
    {
        action?.Invoke(BotCommandScopes);
        return this;
    }

    #endregion


    #region "Step 6 (Serialization)"

    public ILanguageSelectionStage NoSerialization()
    {
        return this;
    }

    public ILanguageSelectionStage UseSerialization(IStateMachine machine)
    {
        _stateMachine = machine;
        return this;
    }

    /// <summary>
    /// Uses the application runtime path to load and write a states.json file.
    /// </summary>
    /// <returns></returns>
    public ILanguageSelectionStage UseJSON()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "states.json");
        _stateMachine = new JsonStateMachine(path);
        return this;
    }

    /// <summary>
    /// Uses the given path to load and write a states.json file.
    /// </summary>
    /// <returns></returns>
    public ILanguageSelectionStage UseJSON(string path)
    {
        _stateMachine = new JsonStateMachine(path);
        return this;
    }

    /// <summary>
    /// Uses the application runtime path to load and write a states.json file.
    /// </summary>
    /// <returns></returns>
    public ILanguageSelectionStage UseSimpleJSON()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "states.json");
        _stateMachine = new SimpleJsonStateMachine(path);
        return this;
    }

    /// <summary>
    /// Uses the given path to load and write a states.json file.
    /// </summary>
    /// <returns></returns>
    public ILanguageSelectionStage UseSimpleJSON(string path)
    {
        _stateMachine = new SimpleJsonStateMachine(path);
        return this;
    }

    /// <summary>
    /// Uses the application runtime path to load and write a states.xml file.
    /// </summary>
    /// <returns></returns>
    public ILanguageSelectionStage UseXML()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "states.xml");
        _stateMachine = new XmlStateMachine(path);
        return this;
    }

    /// <summary>
    /// Uses the given path to load and write a states.xml file.
    /// </summary>
    /// <returns></returns>
    public ILanguageSelectionStage UseXML(string path)
    {
        _stateMachine = new XmlStateMachine(path);
        return this;
    }

    #endregion


    #region "Step 7 (Language)"

    public IBuildingStage DefaultLanguage()
    {
        return this;
    }

    public IBuildingStage UseEnglish()
    {
        Default.Language = new English();
        return this;
    }

    public IBuildingStage UseGerman()
    {
        Default.Language = new German();
        return this;
    }

    public IBuildingStage UsePersian()
    {
        Default.Language = new Persian();
        return this;
    }

    public IBuildingStage Custom(Localization language)
    {
        Default.Language = language;
        return this;
    }

    #endregion
}
