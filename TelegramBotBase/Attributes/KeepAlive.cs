using System;

namespace TelegramBotBase.Attributes;

/// <summary>
///     Declares that a session showing this form must not be evicted by the configured session limits
///     (see <see cref="Sessions.SessionLimits" />). It can still be ended manually.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class KeepAlive : Attribute
{
}
