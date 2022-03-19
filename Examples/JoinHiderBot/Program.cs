using System;
using TelegramBotBase.Builder;

namespace JoinHiderBot
{
    class Program
    {
        static void Main(string[] args)
        {

            String apiKey = "";

            var bot = BotBaseBuilder.Create()
                                    .QuickStart<forms.Start>(apiKey)
                                    .Build();

            bot.Start();

            Console.ReadLine();
        }
    }
}
