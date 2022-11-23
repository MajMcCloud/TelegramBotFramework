using System;
using TelegramBotBase.Builder;

namespace SystemCommandsBot
{
    class Program
    {
        public static config.Config BotConfig { get; set; }


        static void Main(string[] args)
        {

            BotConfig = config.Config.load();

            if (BotConfig.ApiKey == null || BotConfig.ApiKey.Trim() == "")
            {
                Console.WriteLine("Config created...");
                Console.ReadLine();
                return;
            }

            var bot = BotBaseBuilder.Create()
                                    .QuickStart<forms.StartForm>(BotConfig.ApiKey)
                                    .Build();

            bot.Start();

            Console.WriteLine("Bot started");

            Console.ReadLine();


            bot.Stop();


        }
    }
}
