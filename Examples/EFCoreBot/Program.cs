using EFCoreBot;
using EFCoreBot.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotBase.Builder;

var serviceCollection = new ServiceCollection()
    .AddDbContext<BotDbContext>(x => x.UseInMemoryDatabase("TelegramBotBase"));

var serviceProvider = serviceCollection.BuildServiceProvider();

var bot = BotBaseBuilder.Create(Environment.GetEnvironmentVariable("API_KEY") ??
                                    throw new Exception("API_KEY is not set"))
                        .WithServiceProvider<StartForm>(serviceProvider)
                        .NoCommands()
                        .Build();

await bot.Start();
await Task.Delay(-1);
