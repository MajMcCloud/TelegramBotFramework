// ClubMateBot demo - SKILL.md 3.3 (middleware pipeline): a small authorization gate.
using TelegramBotBase.MessageLoops;

namespace ClubMateBot.Middleware;

/// <summary>
///     Pipeline step that runs before anything touches a form.
///     An empty <see cref="BotConfig.AllowedUsers" /> set means "everybody may use the bot",
///     which is the sensible default for a demo.
/// </summary>
public static class AuthGate
{
    public static Func<MessageContainer, Func<Task>, Task> Create(BotConfig config)
    {
        return async (container, next) =>
        {
            var update = container.UpdateResult;
            var fromId = update.Message?.From?.Id ?? update.RawData.CallbackQuery?.From?.Id;

            if (config.Verbose)
            {
                Console.WriteLine($"[gate] {update.RawData.Type} from {update.DeviceId} (user {fromId})");
            }

            if (config.AllowedUsers.Count > 0 && (fromId is null || !config.AllowedUsers.Contains(fromId.Value)))
            {
                // The update is simply dropped - the pipeline stops here, no form is ever opened.
                if (fromId is not null)
                {
                    await container.DeviceSession.Send("This demo bot is currently locked to a few user ids.");
                }

                return;
            }

            await next();
        };
    }
}
