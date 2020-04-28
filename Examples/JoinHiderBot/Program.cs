using System;

namespace JoinHiderBot
{
    class Program
    {
        static void Main(string[] args)
        {

            String apiKey = "";

            var bot = new TelegramBotBase.BotBase<forms.Start>(apiKey);

            bot.Start();

            Console.ReadLine();
        }
    }
}
