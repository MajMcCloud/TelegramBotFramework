using System;

namespace TelegramBotBase.Args;

public class PromptDialogCompletedEventArgs : EventArgs
{
    public object Tag { get; set; }

    public string Value { get; set; }
}