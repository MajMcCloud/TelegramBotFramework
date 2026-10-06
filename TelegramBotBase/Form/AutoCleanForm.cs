using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using TelegramBotBase.Args;
using TelegramBotBase.Attributes;
using TelegramBotBase.Base;
using TelegramBotBase.Enums;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.Form;

/// <summary>
///     A form which cleans up old messages sent within
/// </summary>
public class AutoCleanForm : FormBase
{
    public AutoCleanForm()
    {
        OldMessages = new List<int>();
        DeleteMode = EDeleteMode.OnEveryCall;
        DeleteSide = EDeleteSide.BotOnly;

        Init += AutoCleanForm_Init;

        Closed += AutoCleanForm_Closed;
    }

    [SaveState] public List<int> OldMessages { get; set; }

    [SaveState] public EDeleteMode DeleteMode { get; set; }

    [SaveState] public EDeleteSide DeleteSide { get; set; }

    private Task AutoCleanForm_Init(object sender, InitEventArgs e)
    {
        if (Device == null)
        {
            return Task.CompletedTask;
        }

        Device.MessageSent += Device_MessageSent;

        Device.MessageReceived += Device_MessageReceived;

        Device.MessageDeleted += Device_MessageDeleted;
        return Task.CompletedTask;
    }

    private void Device_MessageDeleted(object sender, MessageDeletedEventArgs e)
    {
        if (OldMessages.Contains(e.MessageId))
        {
            OldMessages.Remove(e.MessageId);
        }
    }

    private void Device_MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        if (DeleteSide == EDeleteSide.BotOnly)
        {
            return;
        }

        OldMessages.Add(e.Message.MessageId);
    }

    private Task Device_MessageSent(object sender, MessageSentEventArgs e)
    {
        if (DeleteSide == EDeleteSide.UserOnly)
        {
            return Task.CompletedTask;
        }

        OldMessages.Add(e.Message.MessageId);
        return Task.CompletedTask;
    }

    public override async Task PreLoad(MessageResult message)
    {
        if (DeleteMode != EDeleteMode.OnEveryCall)
        {
            return;
        }

        await MessageCleanup();
    }

    /// <summary>
    ///     Adds a message to this of removable ones
    /// </summary>
    /// <param name="Id"></param>
    public void AddMessage(Message m)
    {
        OldMessages.Add(m.MessageId);
    }


    /// <summary>
    ///     Adds a message to this of removable ones
    /// </summary>
    /// <param name="Id"></param>
    public void AddMessage(int messageId)
    {
        OldMessages.Add(messageId);
    }

    /// <summary>
    ///     Keeps the message by removing it from the list
    /// </summary>
    /// <param name="id"></param>
    public void LeaveMessage(int id)
    {
        OldMessages.Remove(id);
    }

    /// <summary>
    ///     Keeps the last sent message
    /// </summary>
    public void LeaveLastMessage()
    {
        if (OldMessages.Count == 0)
        {
            return;
        }

        OldMessages.RemoveAt(OldMessages.Count - 1);
    }

    private async Task AutoCleanForm_Closed(object sender, EventArgs e)
    {
        if (DeleteMode != EDeleteMode.OnLeavingForm)
        {
            return;
        }

        await MessageCleanup();
    }

    /// <summary>
    ///     Cleans up all remembered messages.
    /// </summary>
    /// <returns></returns>
    public async Task MessageCleanup()
    {
        var old_messages = OldMessages.ToList();

        foreach (var m in old_messages)
        {
            try
            {
                await Device.Dispatch(a => Device.DeleteMessage(m));
            }
            catch (ApiRequestException req) when (req.ErrorCode == 400)
            {
                // Message already deleted, ignore
            }
        }

        OldMessages.Clear();
    }
}
