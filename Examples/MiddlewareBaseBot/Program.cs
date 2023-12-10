using Telegram.Bot.Types.Enums;
using TelegramBotBase;
using TelegramBotBase.Builder;
using TelegramBotBase.MessageLoops.Extensions;

public class Program
{
    private static async Task Main(string[] args)
    {
        var bot = GetPhotoBot();

        await bot.Start();

        Console.WriteLine("Bot started :)");

        Console.ReadLine();
    }

    /// <summary>
    ///     Creates a bot with middleware message loop and authentication for admin user
    /// </summary>
    private static BotBase GetAdminBot()
    {
        var bot = BotBaseBuilder
                    .Create()
                    .WithAPIKey(Environment.GetEnvironmentVariable("API_KEY") ?? throw new Exception("API_KEY is not set"))
                    .MiddlewareMessageLoop(
                        messageLoop =>
                            messageLoop
                                .Use(async (container, next) =>
                                {
                                    var updateResult = container.UpdateResult;
                                    if (updateResult.Message is not null)
                                    {
                                        if (updateResult.Message.From is not null)
                                        {
                                            var fromId = updateResult.Message.From.Id;

                                            if (fromId == 1)
                                            {
                                                await next();
                                            }
                                        }
                                    }

                                    return;
                                })
                                .UseValidUpdateTypes(
                                    UpdateType.Message,
                                    UpdateType.EditedMessage,
                                    UpdateType.CallbackQuery)
                                .UseBotCommands()
                                .UseForms())
                    .WithStartForm<StartForm>()
                    .NoProxy()
                    .DefaultCommands()
                    .NoSerialization()
                    .UsePersian()
                    .Build();

        return bot;
    }

    /// <summary>
    ///     Creates a bot with middleware message loop for handle inline queries
    /// </summary>
    private static BotBase GetInlineQueryBot()
    {
        var bot = BotBaseBuilder
                    .Create()
                    .WithAPIKey(Environment.GetEnvironmentVariable("API_KEY") ?? throw new Exception("API_KEY is not set"))
                    .MiddlewareMessageLoop(
                        messageLoop =>
                            messageLoop
                                .UseValidUpdateTypes(UpdateType.InlineQuery)
                                .Use(async (container, next) =>
                                {
                                    var query = container.UpdateResult.RawData.InlineQuery.Query;

                                    if (!string.IsNullOrWhiteSpace(query))
                                    {
                                        // logic

                                        await next();
                                    }

                                    return;
                                }))
                    .WithStartForm<StartForm>()
                    .NoProxy()
                    .DefaultCommands()
                    .NoSerialization()
                    .UsePersian()
                    .Build();

        return bot;
    }

    /// <summary>
    ///     Creates a bot with middleware message loop like form base message loop
    /// </summary>
    private static BotBase GetFormBaseBot()
    {
        var bot = BotBaseBuilder
                            .Create()
                            .WithAPIKey(Environment.GetEnvironmentVariable("API_KEY") ?? throw new Exception("API_KEY is not set"))
                            .MiddlewareMessageLoop(
                                messageLoop =>
                                    messageLoop
                                        .UseValidUpdateTypes(
                                            UpdateType.Message,
                                            UpdateType.EditedMessage,
                                            UpdateType.CallbackQuery)
                                        .UseBotCommands()
                                        .UseForms())
                            // OR instead of UseForms 
                            // .UsePreLoad()
                            // .UseLoad()
                            // .UseAllAttachments()
                            // .UseActions()
                            // .UseRender()
                            .WithStartForm<StartForm>()
                            .NoProxy()
                            .DefaultCommands()
                            .NoSerialization()
                            .UsePersian()
                            .Build();

        return bot;
    }

    /// <summary>
    ///     Creates a bot with middleware message loop like form base message loop
    /// </summary>
    private static BotBase GetFullBot()
    {
        var bot = BotBaseBuilder
                            .Create()
                            .WithAPIKey(Environment.GetEnvironmentVariable("API_KEY") ?? throw new Exception("API_KEY is not set"))
                            .MiddlewareMessageLoop(
                                messageLoop =>
                                    messageLoop
                                        .UseBotCommands()
                                        .UseForms())
                            .WithStartForm<StartForm>()
                            .NoProxy()
                            .DefaultCommands()
                            .NoSerialization()
                            .UsePersian()
                            .Build();

        return bot;
    }

    /// <summary>
    ///     Creates a bot with middleware message loop like minimal message loop
    /// </summary>
    private static BotBase GetMinimalBot()
    {
        var bot = BotBaseBuilder
                            .Create()
                            .WithAPIKey(Environment.GetEnvironmentVariable("API_KEY") ?? throw new Exception("API_KEY is not set"))
                            .MiddlewareMessageLoop(
                                messageLoop =>
                                    messageLoop
                                        .UseLoad())
                            .WithStartForm<StartForm>()
                            .NoProxy()
                            .DefaultCommands()
                            .NoSerialization()
                            .UsePersian()
                            .Build();

        return bot;
    }

    private static BotBase GetPhotoBot()
    {
        var bot = BotBaseBuilder
                    .Create()
                    .WithAPIKey(Environment.GetEnvironmentVariable("API_KEY") ?? throw new Exception("API_KEY is not set"))
                    .MiddlewareMessageLoop(
                        messageLoop =>
                            messageLoop
                                .UseAttachments(MessageType.Photo))
                    .WithStartForm<StartForm>()
                    .NoProxy()
                    .DefaultCommands()
                    .NoSerialization()
                    .UsePersian()
                    .Build();

        return bot;
    }
}