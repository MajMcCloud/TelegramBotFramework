namespace TelegramBotBase.RequestDispatchers;

/// <summary>Result of a non-blocking attempt to take a dispatch slot</summary>
public enum SlotAttempt
{
    /// <summary>The slot was taken and must be released with <c>ReleaseSpot</c></summary>
    Acquired,

    /// <summary>A shared limit (concurrency or global rate) is exhausted, so no request can start right now</summary>
    BlockedGlobally,

    /// <summary>Only this session's limit (per-chat or per-group) is exhausted, other sessions may still proceed</summary>
    BlockedForSession
}
