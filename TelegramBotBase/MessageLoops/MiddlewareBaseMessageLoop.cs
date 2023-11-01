using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.MessageLoops;

/// <summary>
///     This message loop based on middleware pattern
/// </summary>
public sealed class MiddlewareBaseMessageLoop : IMessageLoopFactory
{
    private List<Func<MessageContainer, Func<Task>, Task>> Middlewares = new();

    public event EventHandler<UnhandledCallEventArgs> UnhandledCall;

    public async Task MessageLoop(BotBase bot, DeviceSession session, UpdateResult ur, MessageResult mr)
    {
        ur.Device = session;
        mr.Device = session;

        var messageContainer = new MessageContainer(bot, session, ur, mr);

        if (Middlewares.Any())
        {
            int middlewareIndex = 0;

            await InvokeMiddleware(messageContainer, middlewareIndex);
        }
    }

    /// <summary>
    ///     Invokes middleware with index
    /// </summary>
    private async Task InvokeMiddleware(MessageContainer messageContainer, int middlewareIndex)
    {
        await Middlewares[middlewareIndex]
            .Invoke(messageContainer, async () =>
            {
                int nextMiddlewareIndex = middlewareIndex + 1;
                if (nextMiddlewareIndex < Middlewares.Count)
                {
                    await InvokeMiddleware(messageContainer, nextMiddlewareIndex);
                }
            });
    }

    /// <summary>
    ///     Adds a new middleware
    /// </summary>
    public void AddMiddleware(Func<MessageContainer, Func<Task>, Task> middleware)
    {
        Middlewares.Add(middleware);
    }
}

public struct MessageContainer
{
    public BotBase BotBase { get; private set; }
    public DeviceSession DeviceSession { get; private set; }
    public UpdateResult UpdateResult { get; private set; }
    public MessageResult MessageResult { get; private set; }

    public MessageContainer(BotBase botBase, DeviceSession deviceSession, UpdateResult updateResult, MessageResult messageResult)
    {
        BotBase = botBase;
        DeviceSession = deviceSession;
        UpdateResult = updateResult;
        MessageResult = messageResult;
    }
}