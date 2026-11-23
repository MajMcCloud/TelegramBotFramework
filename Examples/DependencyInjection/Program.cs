using DependencyInjection.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotBase.Builder;

namespace DependencyInjection
{
    internal class Program
    {
        static async Task Main(string[] args)
        {


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

        }
    }
}