using System;

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

            var bot = new TelegramBotBase.BotBase<forms.StartForm>(BotConfig.ApiKey);

            bot.Start();

            Console.WriteLine("Bot started");

            Console.ReadLine();


            bot.Stop();


        }
    }
}
