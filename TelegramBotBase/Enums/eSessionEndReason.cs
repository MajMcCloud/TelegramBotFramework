namespace TelegramBotBase.Enums;

/// <summary>
///     Describes why a device session has been ended.
/// </summary>
public enum ESessionEndReason
{
    /// <summary>
    ///     The session has been ended by the application (<see cref="SessionManager.EndSessionAsync(long)" />).
    ///     The active form gets closed (<c>OnClosed</c>) and its saved state is discarded.
    /// </summary>
    Manual = 1,

    /// <summary>
    ///     The session has been evicted because it was idle longer than the configured idle timeout.
    ///     Forms are disposed silently (no <c>OnClosed</c>) and the state gets persisted if a state machine is configured.
    /// </summary>
    Idle = 2,

    /// <summary>
    ///     The session has been evicted because the configured maximum number of sessions was exceeded.
    ///     Forms are disposed silently (no <c>OnClosed</c>) and the state gets persisted if a state machine is configured.
    /// </summary>
    Capacity = 3
}
