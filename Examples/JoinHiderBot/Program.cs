using JoinHiderBot.Forms;
using TelegramBotBase.Builder;

namespace JoinHiderBot;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var bot = BotBaseBuilder.Create()
                                .QuickStart<Start>(Environment.GetEnvironmentVariable("API_KEY") ??
                                                   throw new Exception("API_KEY is not set"))
                                .Build();

        await bot.Start();

        Console.ReadLine();
    }
}
