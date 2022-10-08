using System;
using System.ComponentModel;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.MessageLoops;

/// <summary>
///     This is a minimal message loop which will react to all update types and just calling the Load method.
/// </summary>
public class MinimalMessageLoop : IMessageLoopFactory
{
    private static readonly object EvUnhandledCall = new();

    private readonly EventHandlerList _events = new();

    public async Task MessageLoop(BotBase bot, DeviceSession session, UpdateResult ur, MessageResult mr)
    {
        var update = ur.RawData;


        mr.Device = session;

        var activeForm = session.ActiveForm;

        //Loading Event
        await activeForm.Load(mr);
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
}
