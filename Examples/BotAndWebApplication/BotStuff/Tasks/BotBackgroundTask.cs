using TelegramBotBase;

namespace BotAndWebApplication.BotStuff.Tasks;

internal sealed class BotBackgroundTask : IHostedService
{
    private readonly BotBase _bot;
    private readonly ILogger<BotBackgroundTask> _logger;
    private readonly TelegramWebhook? _webhook;

    public BotBackgroundTask(BotBase bot, ILogger<BotBackgroundTask> logger, TelegramWebhook? webhook = null)
    {
        _bot = bot;
        _logger = logger;
        _webhook = webhook;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bot is starting.");

        await _bot.UploadBotCommands();
        await _bot.Start(cancellationToken);
        _webhook?.Start();

        _logger.LogInformation("Bot has started.");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bot is stopping.");

        try
        {
            if (_webhook != null)
            {
                await _webhook.StopAsync();
            }
            else
            {
                await _bot.SentToAll("Bot will shut down now.");
            }
        }
        finally
        {
            await _bot.Stop();
        }

        _logger.LogInformation("Bot has stopped.");
    }
}
