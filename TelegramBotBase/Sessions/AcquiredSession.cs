using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Sessions;

/// <summary>
///     A session acquired for processing (see <c>SessionManager.AcquireSession</c>).
/// </summary>
internal readonly struct AcquiredSession
{
    public AcquiredSession(IDeviceSession session, bool created, bool restored)
    {
        Session = session;
        Created = created;
        Restored = restored;
    }

    public IDeviceSession Session { get; }

    /// <summary>
    ///     The session has been created by this acquisition (started or restored).
    /// </summary>
    public bool Created { get; }

    /// <summary>
    ///     The session has been restored from a saved state instead of being started with the start form.
    /// </summary>
    public bool Restored { get; }
}
