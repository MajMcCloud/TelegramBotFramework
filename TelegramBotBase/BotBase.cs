using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Commands;
using TelegramBotBase.Enums;
using TelegramBotBase.Exceptions;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Sessions;
using Console = TelegramBotBase.Tools.Console;

namespace TelegramBotBase;

/// <summary>
///     Bot base class for full Device/Context and message handling
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class BotBase
{
    internal BotBase(string apiKey, MessageClient client)
    {
        ApiKey = apiKey;
        Client = client;

        SystemSettings = new Dictionary<ESettings, uint>();

        SetSetting(ESettings.MaxNumberOfRetries, 5);
        SetSetting(ESettings.NavigationMaximum, 10);
        SetSetting(ESettings.LogAllMessages, false);
        SetSetting(ESettings.SkipAllMessages, false);
        SetSetting(ESettings.SaveSessionsOnConsoleExit, false);
        SetSetting(ESettings.HandleRelationChanges, true);

        BotCommandScopes = new List<BotCommandScopeGroup>();

        Sessions = new SessionManager(this);
    }

    public MessageClient Client { get; }

    /// <summary>
    ///     Your TelegramBot APIKey
    /// </summary>
    public string ApiKey { get; }

    /// <summary>
    ///     List of all running/active sessions
    /// </summary>
    public SessionManager Sessions { get; }

    /// <summary>
    ///     Contains System commands which will be available at everytime and didnt get passed to forms, i.e. /start
    /// </summary>
    public List<BotCommandScopeGroup> BotCommandScopes { get; internal set; }


    /// <summary>
    ///     Enable the SessionState (you need to implement on call forms the IStateForm interface)
    /// </summary>
    public IStateMachine StateMachine { get; internal set; }

    /// <summary>
    ///     Offers functionality to manage the creation process of the start form.
    /// </summary>
    public IStartFormFactory StartFormFactory { get; internal set; }

    /// <summary>
    ///     Contains the message loop factory, which cares about "message-management."
    /// </summary>
    public IMessageLoopFactory MessageLoopFactory { get; internal set; }

    /// <summary>
    ///     All internal used settings.
    /// </summary>
    public Dictionary<ESettings, uint> SystemSettings { get; }


    /// <summary>
    ///     Start your Bot
    /// </summary>
    public async Task Start()
    {
        if (Client == null)
        {
            return;
        }

        Client.MessageLoop += Client_MessageLoop;

        if (StateMachine != null)
        {
            await Sessions.LoadSessionStates(StateMachine);
        }

        // Enable auto session saving
        if (GetSetting(ESettings.SaveSessionsOnConsoleExit, false))
        {
            // should be waited until finish
            Console.SetHandler(() => { Sessions.SaveSessionStates().GetAwaiter().GetResult(); });
        }

        DeviceSession.MaxNumberOfRetries = GetSetting(ESettings.MaxNumberOfRetries, 5);

        Client.StartReceiving();
    }


    private async Task Client_MessageLoop(object sender, UpdateResult e)
    {
        try
        {
            var ds = Sessions.GetSession(e.DeviceId);
            if (ds == null)
            {
                ds = await Sessions.StartSession(e.DeviceId);
                ds.LastMessage = e.Message;

                OnSessionBegins(new SessionBeginEventArgs(e.DeviceId, ds));
            }

            e.Device = ds;

            var mr = new MessageResult(e.RawData);
            mr.Device = ds;

            //Check if user blocked or unblocked the bot
            if (e.RawData.Type == UpdateType.MyChatMember && GetSetting(Enums.ESettings.HandleRelationChanges, true)
                && e?.RawData?.MyChatMember?.NewChatMember is not null)
            {
                OnBotRelationChanged(new BotRelationChangedEventArgs(ds, e, mr));
                return;
            }

            var i = 0;

            //Should formulars get navigated (allow maximum of 10, to dont get loops)
            do
            {
                i++;

                //Reset navigation
                ds.FormSwitched = false;

                await MessageLoopFactory.MessageLoop(this, ds, e, mr);

                mr.IsFirstHandler = false;
            } while (ds.FormSwitched && i < GetSetting(ESettings.NavigationMaximum, 10));
        }
        catch (InvalidServiceProviderConfiguration ex)
        {
            var ds = Sessions.GetSession(e.DeviceId);
            OnException(new SystemExceptionEventArgs(e.Message.Text, e.DeviceId, ds, ex));

            throw;
        }
        catch (Exception ex)
        {
            var ds = Sessions.GetSession(e.DeviceId);
            OnException(new SystemExceptionEventArgs(e.Message.Text, e.DeviceId, ds, ex));
        }
    }


    /// <summary>
    ///     Stop your Bot
    /// </summary>
    public async Task Stop()
    {
        if (Client == null)
        {
            return;
        }

        Client.MessageLoop -= Client_MessageLoop;
        Client.StopReceiving();

        await Sessions.SaveSessionStates();
    }

    /// <summary>
    ///     Send a message to all active Sessions.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task SentToAll(string message)
    {
        if (Client == null)
        {
            return;
        }

        foreach (var s in Sessions.SessionList)
        {
            await Client.TelegramClient.SendMessage(s.Key, message);
        }
    }


    /// <summary>
    ///     This will invoke the full message loop for the device even when no "userevent" like message or action has been
    ///     raised.
    /// </summary>
    /// <param name="deviceId">Contains the device/chat id of the device to update.</param>
    public async Task InvokeMessageLoop(long deviceId)
    {
        var mr = new MessageResult(new Update
        {
            Message = new Message()
        });

        await InvokeMessageLoop(deviceId, mr);
    }

    /// <summary>
    ///     This will invoke the full message loop for the device even when no "userevent" like message or action has been
    ///     raised.
    /// </summary>
    /// <param name="deviceId">Contains the device/chat id of the device to update.</param>
    /// <param name="e"></param>
    public async Task InvokeMessageLoop(long deviceId, MessageResult e)
    {
        try
        {
            var ds = Sessions.GetSession(deviceId);
            e.Device = ds;

            await MessageLoopFactory.MessageLoop(this, ds, new UpdateResult(e.UpdateData, ds), e);
        }
        catch (Exception ex)
        {
            var ds = Sessions.GetSession(deviceId);
            OnException(new SystemExceptionEventArgs(e.Message.Text, deviceId, ds, ex));
        }
    }



    /// <summary>
    /// Returns a list of all bot commands in all configured scopes.
    /// </summary>
    /// <returns></returns>
    public async Task<List<BotCommandScopeGroup>> GetBotCommands()
    {
        var languages = BotCommandScopes.Select(a => a.Language).Distinct().ToArray();

        return await GetBotCommands(languages);
    }

    /// <summary>
    /// Returns a list of all bot commands in all configured scopes and given languages.
    /// </summary>
    /// <param name="additional_languages"></param>
    /// <returns></returns>
    public async Task<List<BotCommandScopeGroup>> GetBotCommands(params string[] additional_languages)
    {
        List<BotCommandScopeGroup> scopes = new List<BotCommandScopeGroup>();

        foreach (var bs in BotCommandScopes)
        {
            foreach(var lang in additional_languages)
            {
                var commands = await Client.GetBotCommands(bs.Scope, lang);
                scopes.Add(new BotCommandScopeGroup(bs.Scope, commands, lang));
            }

        }
        return scopes;
    }

    /// <summary>
    ///     This method will update all local created bot commands to the botfather.
    /// </summary>
    public async Task UploadBotCommands()
    {
        foreach (var bs in BotCommandScopes)
        {
            if (bs.Remove)
            {
                await Client.DeleteBotCommands(bs.Scope, bs.Language);
                continue;
            }

            await Client.SetBotCommands(bs.Commands, bs.Scope, bs.Language);
        }

        BotCommandScopes = BotCommandScopes.Where(a => !a.Remove).ToList();
    }

    /// <summary>
    ///     Searching if parameter is a known command in all configured BotCommandScopes.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public bool IsKnownBotCommand(string command)
    {
        foreach (var scope in BotCommandScopes)
        {
            if (scope.HasCommand(command))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Could set a variety of settings to improve the bot handling.
    /// </summary>
    /// <param name="set"></param>
    /// <param name="value"></param>
    public void SetSetting(ESettings set, uint value)
    {
        SystemSettings[set] = value;
    }

    /// <summary>
    ///     Could set a variety of settings to improve the bot handling.
    /// </summary>
    /// <param name="set"></param>
    /// <param name="value"></param>
    public void SetSetting(ESettings set, bool value)
    {
        SystemSettings[set] = value ? 1u : 0u;
    }

    /// <summary>
    ///     Could get the current value of a setting
    /// </summary>
    /// <param name="set"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public uint GetSetting(ESettings set, uint defaultValue)
    {
        if (!SystemSettings.ContainsKey(set))
        {
            return defaultValue;
        }

        return SystemSettings[set];
    }

    /// <summary>
    ///     Could get the current value of a setting
    /// </summary>
    /// <param name="set"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public bool GetSetting(ESettings set, bool defaultValue)
    {
        if (!SystemSettings.ContainsKey(set))
        {
            return defaultValue;
        }

        return SystemSettings[set] != 0u;
    }


    #region "Events"

    private readonly EventHandlerList _events = new();

    private static readonly object EvSessionBegins = new();

    private static readonly object EvMessage = new();

    public delegate Task BotCommandEventHandler(object sender, BotCommandEventArgs e);

    private static readonly object EvException = new();

    private static readonly object EvUnhandledCall = new();

    private static readonly object EvOnBotRelationChanged = new();


    /// <summary>
    ///     Will be called if a session/context gets started
    /// </summary>
    public event EventHandler<SessionBeginEventArgs> SessionBegins
    {
        add => _events.AddHandler(EvSessionBegins, value);
        remove => _events.RemoveHandler(EvSessionBegins, value);
    }

    public void OnSessionBegins(SessionBeginEventArgs e)
    {
        (_events[EvSessionBegins] as EventHandler<SessionBeginEventArgs>)?.Invoke(this, e);
    }

    /// <summary>
    ///     Will be called on incoming message
    /// </summary>
    public event EventHandler<MessageIncomeEventArgs> Message
    {
        add => _events.AddHandler(EvMessage, value);
        remove => _events.RemoveHandler(EvMessage, value);
    }

    public void OnMessage(MessageIncomeEventArgs e)
    {
        (_events[EvMessage] as EventHandler<MessageIncomeEventArgs>)?.Invoke(this, e);
    }

    /// <summary>
    ///     Will be called if a bot command gets raised
    /// </summary>
    public event BotCommandEventHandler BotCommand;


    public async Task OnBotCommand(BotCommandEventArgs e)
    {
        if (BotCommand != null)
        {
            await BotCommand(this, e);
        }
    }

    /// <summary>
    ///     Will be called on an inner exception
    /// </summary>
    public event EventHandler<SystemExceptionEventArgs> Exception
    {
        add => _events.AddHandler(EvException, value);
        remove => _events.RemoveHandler(EvException, value);
    }

    public void OnException(SystemExceptionEventArgs e)
    {
        (_events[EvException] as EventHandler<SystemExceptionEventArgs>)?.Invoke(this, e);
    }

    /// <summary>
    ///     Will be called if no form handled this call
    /// </summary>
    public event EventHandler<UnhandledCallEventArgs> UnhandledCall
    {
        add => _events.AddHandler(EvUnhandledCall, value);
        remove => _events.RemoveHandler(EvUnhandledCall, value);
    }

    public void OnUnhandledCall(UnhandledCallEventArgs e)
    {
        (_events[EvUnhandledCall] as EventHandler<UnhandledCallEventArgs>)?.Invoke(this, e);
    }


    /// <summary>
    ///     Will be called if the relation between the bot and the user has changed.
    /// </summary>
    public event EventHandler<BotRelationChangedEventArgs> BotRelationChanged
    {
        add => _events.AddHandler(EvOnBotRelationChanged, value);
        remove => _events.RemoveHandler(EvOnBotRelationChanged, value);
    }

    public void OnBotRelationChanged(BotRelationChangedEventArgs e)
    {
        (_events[EvOnBotRelationChanged] as EventHandler<BotRelationChangedEventArgs>)?.Invoke(this, e);
    }


    #endregion
}
