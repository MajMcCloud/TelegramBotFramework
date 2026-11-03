using TelegramBotBase.Enums;

namespace TelegramBotBase.Sessions;

/// <summary>
///     Tracks how many operations currently use a session and if it has been ended.
///     All fields are guarded by locking the instance.
/// </summary>
internal sealed class SessionLease
{
    /// <summary>
    ///     Number of operations (updates, message loop invocations) currently using the session.
    /// </summary>
    public int Active;

    /// <summary>
    ///     The session has been ended and removed from the session list. It can't be acquired anymore.
    /// </summary>
    public bool Ended;

    /// <summary>
    ///     The forms of the session have been (or are being) disposed.
    /// </summary>
    public bool Disposed;

    /// <summary>
    ///     Why the session has been ended.
    /// </summary>
    public ESessionEndReason Reason;
}
