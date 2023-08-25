using System.Threading.Tasks;
using TelegramBotBase.Args;

namespace TelegramBotBase.Interfaces;

/// <summary>
///     Is used to save specific fields into a session state to survive restarts or unhandled exceptions and crashes.
/// </summary>
public interface IStateForm
{
    Task LoadState(LoadStateEventArgs e);

    Task SaveState(SaveStateEventArgs e);
}
