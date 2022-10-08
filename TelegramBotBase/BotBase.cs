using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Enums;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Sessions;
using Console = TelegramBotBase.Tools.Console;

namespace TelegramBotBase;

/// <summary>
///     Bot base class for full Device/Context and Messagehandling
/// </summary>
/// <typeparam name="T"></typeparam>
public class BotBase
{
    public BotBase()
    {
        SystemSettings = new Dictionary<ESettings, uint>();

        SetSetting(ESettings.MaxNumberOfRetries, 5);
        SetSetting(ESettings.NavigationMaximum, 10);
        SetSetting(ESettings.LogAllMessages, false);
        SetSetting(ESettings.SkipAllMessages, false);
        SetSetting(ESettings.SaveSessionsOnConsoleExit, false);

        BotCommandScopes = new Dictionary<BotCommandScope, List<BotCommand>>();

        Sessions = new SessionBase
        {
            BotBase = this
        };
    }

    public MessageClient Client { get; set; }

    /// <summary>
    ///     Your TelegramBot APIKey
    /// </summary>
    public string ApiKey { get; set; } = "";

    /// <summary>
    ///     List of all running/active sessions
    /// </summary>
    public SessionBase Sessions { get; set; }

    /// <summary>
    ///     Contains System commands which will be available at everytime and didnt get passed to forms, i.e. /start
    /// </summary>
    public Dictionary<BotCommandScope, List<BotCommand>> BotCommandScopes { get; set; } = new();


    /// <summary>
    ///     Enable the SessionState (you need to implement on call forms the IStateForm interface)
    /// </summary>
    public IStateMachine StateMachine { get; set; }

    /// <summary>
    ///     Offers functionality to manage the creation process of the start form.
    /// </summary>
    public IStartFormFactory StartFormFactory { get; set; }

    /// <summary>
    ///     Contains the message loop factory, which cares about "message-management."
    /// </summary>
    public IMessageLoopFactory MessageLoopFactory { get; set; }

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

        //Enable auto session saving
        if (GetSetting(ESettings.SaveSessionsOnConsoleExit, false))
        {
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
                e.Device = ds;
                ds.LastMessage = e.RawData.Message;

                OnSessionBegins(new SessionBeginEventArgs(e.DeviceId, ds));
            }

            var mr = new MessageResult(e.RawData);

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
            await Client.TelegramClient.SendTextMessageAsync(s.Key, message);
        }
    }


    /// <summary>
    ///     This will invoke the full message loop for the device even when no "userevent" like message or action has been
    ///     raised.
    /// </summary>
    /// <param name="deviceId">Contains the device/chat id of the device to update.</param>
    public async Task InvokeMessageLoop(long deviceId)
    {
        var mr = new MessageResult
        {
            UpdateData = new Update
            {
                Message = new Message()
            }
        };

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
            //await Client_Loop(this, e);
        }
        catch (Exception ex)
        {
            var ds = Sessions.GetSession(deviceId);
            OnException(new SystemExceptionEventArgs(e.Message.Text, deviceId, ds, ex));
        }
    }


    /// <summary>
    ///     Will get invoke on an unhandled call.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void MessageLoopFactory_UnhandledCall(object sender, UnhandledCallEventArgs e)
    {
        OnUnhandledCall(e);
    }

    /// <summary>
    ///     This method will update all local created bot commands to the botfather.
    /// </summary>
    public async Task UploadBotCommands()
    {
        foreach (var bs in BotCommandScopes)
        {
            if (bs.Value != null)
            {
                await Client.SetBotCommands(bs.Value, bs.Key);
            }
            else
            {
                await Client.DeleteBotCommands(bs.Key);
            }
        }
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
            if (scope.Value.Any(a => "/" + a.Command == command))
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

        return SystemSettings[set] == 0u ? false : true;
    }


    #region "Events"

    private readonly EventHandlerList _events = new();

    private static readonly object EvSessionBegins = new();

    private static readonly object EvMessage = new();

    private static object __evSystemCall = new();

    public delegate Task BotCommandEventHandler(object sender, BotCommandEventArgs e);

    private static readonly object EvException = new();

    private static readonly object EvUnhandledCall = new();

    #endregion

    #region "Events"

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
    ///     Will be called on incomming message
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

    #endregion
}
