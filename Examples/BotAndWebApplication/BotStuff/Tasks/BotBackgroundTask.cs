using TelegramBotBase;
using TelegramBotBase.Builder;

namespace BotAndWebApplication.BotStuff.Tasks
{
    public class BotBackgroundTask : IHostedService
    {
        BotBackgroundTask()
        {
            BotBaseInstance = BotBaseBuilder.Create()
                                .WithAPIKey(Environment.GetEnvironmentVariable("API_KEY") ??
                                    throw new Exception("API_KEY is not set"))
                                .DefaultMessageLoop()
                                .WithStartForm<StartForm>()
                                .NoProxy()
                                .DefaultCommands()
                                .NoSerialization()
                                .UseEnglish()
                                .Build();
        }

        public BotBackgroundTask(ILogger<BotBackgroundTask> logger) : this()
        {
            Logger = logger;
        }

        public ILogger<BotBackgroundTask>? Logger { get; }

        public BotBase BotBaseInstance { get; private set; }




        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (BotBaseInstance == null)
                return;

            Logger?.LogInformation("Bot is starting.");

            await BotBaseInstance.UploadBotCommands();

            await BotBaseInstance.Start();


            Logger?.LogInformation("Bot has been started.");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (BotBaseInstance == null)
                return;

            Logger?.LogInformation("Bot will shut down.");

            //Let all users know that the bot will shut down.

            await BotBaseInstance.SentToAll("Bot will shut down now.");

            await BotBaseInstance.Stop();

            Logger?.LogInformation("Bot has shutted down.");
        }
    }
}
