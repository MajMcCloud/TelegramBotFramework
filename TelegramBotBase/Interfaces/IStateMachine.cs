using System;
using TelegramBotBase.Args;
using TelegramBotBase.Base;

namespace TelegramBotBase.Interfaces;

public interface IStateMachine
{
    Type FallbackStateForm { get; }

    void SaveFormStates(SaveStatesEventArgs e);

    StateContainer LoadFormStates();
}