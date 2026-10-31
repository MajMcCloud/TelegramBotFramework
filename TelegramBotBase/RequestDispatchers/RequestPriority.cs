namespace TelegramBotBase.RequestDispatchers;

/// <summary>
/// Scheduling priority of an outgoing Telegram Bot API request. Lower numeric value is served first
/// whenever requests have to wait for a rate-limit or concurrency slot
/// </summary>
public enum RequestPriority
{
    /// <summary>Must go out before everything else (e.g. answering callback queries)</summary>
    Critical = 0,

    /// <summary>Interactive UI work the user is actively waiting for (edits, form rendering, cleanup)</summary>
    UI = 1,

    /// <summary>Default priority for regular requests</summary>
    Normal = 2,

    /// <summary>Bulk or non-urgent traffic such as broadcasts</summary>
    Background = 3
}
