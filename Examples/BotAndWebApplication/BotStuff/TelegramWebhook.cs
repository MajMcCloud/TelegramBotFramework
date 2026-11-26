using System.Text.Json;
using System.Threading.Channels;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotBase;
using TelegramBotBase.Args;

namespace BotAndWebApplication.BotStuff;

internal sealed class TelegramWebhook
{
    public const string Path = "/bot/webhook";
    private const string SecretHeader = "X-Telegram-Bot-Api-Secret-Token";
    private const int QueueCapacity = 100;
    private const int WorkerCount = 4;

    private readonly Channel<Update> _updates = Channel.CreateBounded<Update>(QueueCapacity);
    private readonly BotBase _bot;
    private readonly ILogger<TelegramWebhook> _logger;
    private Task _processing = Task.CompletedTask;

    public TelegramWebhook(BotBase bot, ILogger<TelegramWebhook> logger)
    {
        _bot = bot;
        _logger = logger;
    }

    public void Start()
    {
        _bot.Exception += LogBotException;
        _processing = Task.WhenAll(Enumerable.Range(0, WorkerCount).Select(_ => ProcessUpdatesAsync()));
    }

    public async Task StopAsync()
    {
        _updates.Writer.TryComplete();
        try
        {
            await _processing;
        }
        finally
        {
            _bot.Exception -= LogBotException;
        }
    }

    public async Task<IResult> HandleAsync(HttpRequest request)
    {
        if (!request.Headers.TryGetValue(SecretHeader, out var secret) || secret.Count != 1 ||
            !_bot.IsValidWebhookSecret(secret[0]))
        {
            return Results.Unauthorized();
        }

        if (!request.HasJsonContentType())
        {
            return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);
        }

        Update? update;
        try
        {
            update = await JsonSerializer.DeserializeAsync<Update>(request.Body, JsonBotAPI.Options,
                                                                   request.HttpContext.RequestAborted);
        }
        catch (JsonException)
        {
            return Results.BadRequest();
        }

        if (update is not { Id: > 0 })
        {
            return Results.BadRequest();
        }

        return _updates.Writer.TryWrite(update)
            ? Results.Ok()
            : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }

    private async Task ProcessUpdatesAsync()
    {
        await foreach (var update in _updates.Reader.ReadAllAsync())
        {
            try
            {
                await _bot.HandleUpdateAsync(update);
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Failed to process Telegram update {UpdateId}.", update.Id);
            }
        }
    }

    private void LogBotException(object? sender, SystemExceptionEventArgs args)
    {
        _logger.LogError(args.Error, "Failed to process Telegram update for device {DeviceId}.", args.DeviceId);
    }
}
