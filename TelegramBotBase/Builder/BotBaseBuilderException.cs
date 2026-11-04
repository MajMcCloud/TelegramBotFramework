using System;
using System.Collections.Generic;
using System.Linq;

namespace TelegramBotBase.Builder;

/// <summary>
///     Thrown by <see cref="BotBaseBuilder.Build" /> when the configuration is invalid.
///     <see cref="Errors" /> contains every problem that was found, not only the first one.
/// </summary>
public sealed class BotBaseBuilderException : InvalidOperationException
{
    internal BotBaseBuilderException(IReadOnlyList<string> errors)
        : base(FormatMessage(errors))
    {
        Errors = errors;
    }

    /// <summary>
    ///     All configuration problems that were found.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    private static string FormatMessage(IEnumerable<string> errors)
    {
        return "The bot configuration is invalid:" + string.Concat(errors.Select(e => Environment.NewLine + " - " + e));
    }
}
