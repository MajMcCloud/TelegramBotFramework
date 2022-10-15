using System;
using System.Threading.Tasks;
using SystemCommandsBot.Forms;
using TelegramBotBase.Builder;

namespace SystemCommandsBot;

internal class Program
{
    public static Config BotConfig { get; set; }

    private static async Task Main(string[] args)
    {
        BotConfig = Config.Load();

        if (BotConfig.ApiKey == null || BotConfig.ApiKey.Trim() == "")
        {
            Console.WriteLine("Config created...");
            Console.ReadLine();
            return;
        }

        var bot = BotBaseBuilder.Create()
                                .QuickStart<StartForm>(BotConfig.ApiKey)
                                .Build();

        await bot.Start();

        Console.WriteLine("Bot started");
        Console.ReadLine();


        await bot.Stop();
    }
}
