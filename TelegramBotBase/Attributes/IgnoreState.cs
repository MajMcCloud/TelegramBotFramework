using System;

namespace TelegramBotBase.Attributes;

/// <summary>
///     Declares that this class should not be getting serialized
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class IgnoreState : Attribute
{
}