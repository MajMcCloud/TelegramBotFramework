﻿using System;
using System.Collections.Generic;
using Telegram.Bot.Types;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Args;

/// <summary>
///     Base class for given bot command results
/// </summary>
public class BotCommandEventArgs : EventArgs
{
    public BotCommandEventArgs()
    {
    }

    public BotCommandEventArgs(string command, List<string> parameters, Message message, long deviceId,
                               IDeviceSession device)
    {
        Command = command;
        Parameters = parameters;
        OriginalMessage = message;
        DeviceId = deviceId;
        Device = device;
    }

    public string Command { get; set; }

    public List<string> Parameters { get; set; }

    public long DeviceId { get; set; }

    public IDeviceSession Device { get; set; }

    public bool Handled { get; set; } = false;

    public Message OriginalMessage { get; set; }
}