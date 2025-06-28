﻿using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Base;

public class ResultBase : EventArgs
{
    public IDeviceSession Device { get; set; }

    public virtual long DeviceId { get; set; }

    public virtual int MessageId => Message?.MessageId ?? 0;

    public virtual Message Message { get; set; }

    /// <summary>
    ///     Deletes the current message
    /// </summary>
    /// <param name="messageId"></param>
    /// <returns></returns>
    public virtual async Task DeleteMessage()
    {
        await DeleteMessage(MessageId);
    }

    /// <summary>
    ///     Deletes the current message or the given one.
    /// </summary>
    /// <param name="messageId"></param>
    /// <returns></returns>
    public virtual async Task DeleteMessage(int messageId = -1)
    {
        try
        {
            await Device.Client.TelegramClient.DeleteMessage(DeviceId, messageId == -1 ? MessageId : messageId);
        }
        catch
        {
        }
    }
}
