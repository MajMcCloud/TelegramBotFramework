using System;
using SystemCommandsBot.config;
using SystemCommandsBot.forms;
using TelegramBotBase.Builder;

namespace SystemCommandsBot
{
    internal class Program
    {
        public static Config BotConfig { get; set; }


        private static void Main(string[] args)
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

            bot.Start();

            Console.WriteLine("Bot started");

            Console.ReadLine();


            bot.Stop();


        }
    }
}
