using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using BotAndWebApplication.BotStuff;
using BotAndWebApplication.BotStuff.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Builder;
using TelegramBotBase.Tests.Infrastructure;

namespace TelegramBotBase.Tests;

public class WebhookTests
{
    private const string BotToken = "1234567890:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
    private const string Url = "https://example.com/bot/webhook";
    private const string Secret = "webhook_secret-42";
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);

    [Fact]
    public void Build_ReportsInvalidWebhookSettingsAndConflictingReceivingMode()
    {
        var builder = BotBaseBuilder.Create(BotToken)
                                    .WithStartForm<TrackingForm>()
                                    .WithWebhook("http://example.com/bot", "invalid secret", maxConnections: 101)
                                    .UseThreadPool();

        var error = Assert.Throws<BotBaseBuilderException>(() => builder.Build());

        Assert.Contains(error.Errors, message => message.Contains("HTTPS"));
        Assert.Contains(error.Errors, message => message.Contains("secret token"));
        Assert.Contains(error.Errors, message => message.Contains("maximum connections"));
        Assert.Contains(error.Errors, message => message.Contains("UseThreadPool"));
        Assert.DoesNotContain("invalid secret", error.Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StartAndStop_UseWebhookRegistrationInsteadOfPolling(bool deleteOnStop)
    {
        using var api = new TestTelegramApi();
        using var httpClient = new HttpClient(api);
        var bot = CreateBuilder(httpClient, new TestMessageLoop())
                  .WithWebhook(Url, Secret, maxConnections: 5, deleteOnStop: deleteOnStop)
                  .ConfigureNetworking(options => options.DropPendingUpdates = true)
                  .Build();
        bot.Client.AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery];

        Assert.True(bot.IsValidWebhookSecret(Secret));
        Assert.False(bot.IsValidWebhookSecret(null));
        Assert.False(bot.IsValidWebhookSecret("Webhook_secret-42"));
        Assert.False(bot.IsValidWebhookSecret($"{Secret},{Secret}"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => bot.HandleUpdateAsync(CreateUpdate(1, 1)));

        await bot.Start();
        await bot.Start();
        await bot.Stop();
        await bot.Stop();

        var registration = api.Requests.First();
        Assert.Equal("setWebhook", registration.Method);
        using var body = JsonDocument.Parse(registration.Body);
        Assert.Equal(Url, body.RootElement.GetProperty("url").GetString());
        Assert.Equal(Secret, body.RootElement.GetProperty("secret_token").GetString());
        Assert.Equal(5, body.RootElement.GetProperty("max_connections").GetInt32());
        Assert.True(body.RootElement.GetProperty("drop_pending_updates").GetBoolean());
        Assert.Equal(new[] { "message", "callback_query" },
                     body.RootElement.GetProperty("allowed_updates").EnumerateArray().Select(type => type.GetString()));
        Assert.Equal(deleteOnStop ? ["setWebhook", "deleteWebhook"] : new[] { "setWebhook" },
                     api.Requests.Select(request => request.Method));
        await Assert.ThrowsAsync<InvalidOperationException>(() => bot.HandleUpdateAsync(CreateUpdate(1, 1)));
    }

    [Fact]
    public async Task Start_OpensAFreshCertificateOnEachRegistration()
    {
        using var api = new TestTelegramApi();
        using var httpClient = new HttpClient(api);
        var streams = new List<MemoryStream>();
        var bot = CreateBuilder(httpClient, new TestMessageLoop())
                  .WithWebhook(Url, Secret, certificateFactory: () =>
                  {
                      var stream = new MemoryStream("public certificate"u8.ToArray());
                      streams.Add(stream);
                      return new InputFileStream(stream, "public.pem");
                  })
                  .Build();

        await bot.Start();
        await bot.Stop();
        await bot.Start();
        await bot.Stop();

        Assert.Equal(2, streams.Count);
        Assert.All(streams, stream => Assert.False(stream.CanRead));
        Assert.Equal(2, api.Requests.Count);
        Assert.All(api.Requests, request =>
        {
            Assert.Equal("setWebhook", request.Method);
            Assert.Contains("public.pem", request.Body);
            Assert.Contains("public certificate", request.Body);
        });
    }

    [Fact]
    public async Task Start_CanBeRetriedAfterRegistrationFails()
    {
        using var api = new TestTelegramApi { RejectRegistration = true };
        using var httpClient = new HttpClient(api);
        var loop = new TestMessageLoop();
        var bot = CreateBuilder(httpClient, loop).WithWebhook(Url, Secret).Build();

        await Assert.ThrowsAsync<ApiRequestException>(() => bot.Start());
        await Assert.ThrowsAsync<InvalidOperationException>(() => bot.HandleUpdateAsync(CreateUpdate(1, 1)));

        api.RejectRegistration = false;
        await bot.Start();
        await bot.HandleUpdateAsync(CreateUpdate(1, 1));
        await bot.Stop();

        Assert.Equal(1, loop.Invocations);
    }

    [Fact]
    public async Task HandleUpdate_SerializesEachDeviceAndStopDrainsAdmittedUpdates()
    {
        using var api = new TestTelegramApi();
        using var httpClient = new HttpClient(api);
        using var cancellation = new CancellationTokenSource();
        var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var finishFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var received = new ConcurrentQueue<int>();
        var loop = new TestMessageLoop
        {
            Handler = async (_, _, update) =>
            {
                received.Enqueue(update.RawData.Id);
                if (update.RawData.Id == 1)
                {
                    firstStarted.SetResult();
                    await finishFirst.Task;
                }
            }
        };
        var bot = CreateBuilder(httpClient, loop).WithWebhook(Url, Secret).Build();
        await bot.Start();
        bot.Client.MessageLoop += (_, _) => Task.CompletedTask;

        try
        {
            var first = bot.HandleUpdateAsync(CreateUpdate(1, 1));
            await firstStarted.Task.WaitAsync(TestTimeout);
            var second = bot.HandleUpdateAsync(CreateUpdate(1, 2));
            var canceled = bot.HandleUpdateAsync(CreateUpdate(1, 3), cancellation.Token);
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => canceled.WaitAsync(TestTimeout));

            await bot.HandleUpdateAsync(CreateUpdate(2, 4)).WaitAsync(TestTimeout);
            Assert.Equal(new[] { 1, 4 }, received);
            Assert.False(first.IsCompleted);
            Assert.False(second.IsCompleted);

            var stop = bot.Stop();
            Assert.False(stop.IsCompleted);
            await Assert.ThrowsAsync<InvalidOperationException>(() => bot.HandleUpdateAsync(CreateUpdate(2, 5)));

            finishFirst.SetResult();
            await Task.WhenAll(first, second, stop).WaitAsync(TestTimeout);
            Assert.Equal(new[] { 1, 4, 2 }, received);
        }
        finally
        {
            finishFirst.TrySetResult();
            await bot.Stop().WaitAsync(TestTimeout);
        }
    }

    [Fact]
    public async Task Start_StillPollsByDefault()
    {
        using var api = new TestTelegramApi();
        using var httpClient = new HttpClient(api);
        var bot = CreateBuilder(httpClient, new TestMessageLoop()).Build();

        Assert.False(bot.IsValidWebhookSecret(Secret));
        await bot.Start();
        try
        {
            await api.PollStarted.Task.WaitAsync(TestTimeout);
            Assert.Equal("getUpdates", Assert.Single(api.Requests).Method);
            await Assert.ThrowsAsync<InvalidOperationException>(() => bot.HandleUpdateAsync(CreateUpdate(1, 1)));
        }
        finally
        {
            await bot.Stop();
        }

        await api.PollCanceled.Task.WaitAsync(TestTimeout);
    }

    [Fact]
    public async Task WebExample_AuthenticatesAndBoundsDeliveryThenDrainsBeforeSaving()
    {
        using var api = new TestTelegramApi();
        using var httpClient = new HttpClient(api);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var unblock = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var received = new ConcurrentQueue<(int UpdateId, long DeviceId, string Text)>();
        var loop = new TestMessageLoop
        {
            Handler = async (_, _, update) =>
            {
                received.Enqueue((update.RawData.Id, update.DeviceId, update.Message.Text));
                started.TrySetResult();
                await unblock.Task;
            }
        };
        var state = new MemoryStateMachine();
        var bot = CreateBuilder(httpClient, loop).WithWebhook(Url, Secret).UseSerialization(state).Build();
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton(bot);
        builder.Services.AddSingleton<TelegramWebhook>();
        builder.Services.AddHostedService<BotBackgroundTask>();
        using var host = builder.Build();
        var webhook = host.Services.GetRequiredService<TelegramWebhook>();
        await host.StartAsync();

        Task stop = null;
        var accepted = new List<int>();
        try
        {
            Assert.Equal(StatusCodes.Status401Unauthorized, await SendToExample(webhook, StringValues.Empty));
            Assert.Equal(StatusCodes.Status401Unauthorized, await SendToExample(webhook, "wrong"));
            Assert.Equal(StatusCodes.Status401Unauthorized, await SendToExample(webhook, new[] { Secret, Secret }));
            Assert.Equal(StatusCodes.Status415UnsupportedMediaType,
                         await SendToExample(webhook, Secret, contentType: "text/plain"));
            Assert.Equal(StatusCodes.Status400BadRequest, await SendToExample(webhook, Secret, body: "{"));
            Assert.Equal(StatusCodes.Status400BadRequest, await SendToExample(webhook, Secret, body: "{}"));

            Assert.Equal(StatusCodes.Status200OK, await SendToExample(webhook, Secret, updateId: 1));
            accepted.Add(1);
            await started.Task.WaitAsync(TestTimeout);

            const int capacityProbeLimit = 120;
            for (var id = 2; id <= capacityProbeLimit; id++)
            {
                var status = await SendToExample(webhook, Secret, updateId: id);
                if (status == StatusCodes.Status503ServiceUnavailable)
                {
                    break;
                }

                Assert.Equal(StatusCodes.Status200OK, status);
                accepted.Add(id);
            }

            Assert.InRange(accepted.Count, 2, capacityProbeLimit - 1);
            stop = host.StopAsync();
            Assert.False(stop.IsCompleted);
            Assert.Equal(0, state.BulkSaves);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, await SendToExample(webhook, Secret, updateId: 121));
        }
        finally
        {
            unblock.TrySetResult();
            await (stop ?? host.StopAsync()).WaitAsync(TestTimeout);
        }

        Assert.Equal(accepted, received.Select(update => update.UpdateId).Order());
        Assert.All(received, update =>
        {
            Assert.Equal(42, update.DeviceId);
            Assert.Equal("hello", update.Text);
        });
        Assert.Equal(1, state.BulkSaves);
    }

    private static async Task<int?> SendToExample(TelegramWebhook webhook, StringValues secret, int updateId = 1,
                                                   string body = null, string contentType = "application/json")
    {
        var request = new DefaultHttpContext().Request;
        request.ContentType = contentType;
        if (secret.Count > 0)
        {
            request.Headers["X-Telegram-Bot-Api-Secret-Token"] = secret;
        }

        body ??= $$"""
                 {
                   "update_id": {{updateId}},
                   "message": {
                     "message_id": 1,
                     "date": 1,
                     "chat": {"id": 42, "type": "private"},
                     "text": "hello"
                   }
                 }
                 """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Body = stream;
        var result = await webhook.HandleAsync(request);
        return Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode;
    }

    private static IBotBaseBuilder CreateBuilder(HttpClient httpClient, TestMessageLoop loop)
    {
        return BotBaseBuilder.Create(BotToken)
                             .WithStartForm<TrackingForm>()
                             .CustomMessageLoop(loop)
                             .ConfigureNetworking(options => options.UseHttpClient(httpClient));
    }

    private static Update CreateUpdate(long chatId, int updateId)
    {
        return new Update
        {
            Id = updateId,
            Message = new Message { Id = updateId, Chat = new Chat { Id = chatId, Type = ChatType.Private } }
        };
    }

    private sealed class TestTelegramApi : HttpMessageHandler
    {
        public ConcurrentQueue<(string Method, string Body)> Requests { get; } = new();

        public TaskCompletionSource PollStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource PollCanceled { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool RejectRegistration { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                                     CancellationToken cancellationToken)
        {
            var method = request.RequestUri.Segments[^1];
            var body = request.Content == null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Enqueue((method, body));

            if (method == "getUpdates")
            {
                PollStarted.TrySetResult();
                try
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                }
                finally
                {
                    PollCanceled.TrySetResult();
                }
            }

            return RejectRegistration && method == "setWebhook"
                ? new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent("""{"ok":false,"error_code":400,"description":"Invalid webhook"}""")
                }
                : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"ok":true,"result":true}""") };
        }
    }
}
