using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.MessageLoops;

/// <summary>
///     This is a minimal message loop which will react to all update types and just calling the Load method.
/// </summary>
public sealed class MinimalMessageLoop : IMessageLoopFactory
{
    public UpdateType[] ConfigureUpdateTypes()
    {
        return Update.AllTypes;
    }

    public async Task MessageLoop(BotBase bot, IDeviceSession session, UpdateResult ur, MessageResult mr)
    {
        var activeForm = session.ActiveForm;

        //Loading Event
        await activeForm.Load(mr);
    }

}
