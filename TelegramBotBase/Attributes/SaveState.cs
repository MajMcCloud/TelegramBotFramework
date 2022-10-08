using System;

namespace TelegramBotBase.Attributes;

/// <summary>
///     Declares that the field or property should be save and recovered an restart.
/// </summary>
public class SaveState : Attribute
{
    public string Key { get; set; }
}