using ModalDialogNavigation.Forms;
using TelegramBotBase;
using TelegramBotBase.Builder;

namespace ModalDialogNavigation
{
    internal class Program
    {
        private static BotBase __bot;

        static async Task Main(string[] args)
        {
            __bot = BotBaseBuilder.Create()
                              .QuickStart<Start>(Environment.GetEnvironmentVariable("API_KEY") ??
                                                 throw new Exception("API_KEY is not set"))
                              .Build();

            await __bot.Start();

            Console.WriteLine("Telegram Bot started...");

            Console.ReadLine();

            await __bot.Stop();
        }
    }
}
