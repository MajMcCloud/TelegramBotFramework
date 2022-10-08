using System;
using JoinHiderBot.forms;
using TelegramBotBase.Builder;

namespace JoinHiderBot;

internal class Program
{
    private static void Main(string[] args)
    {
        var apiKey = "";

        var bot = BotBaseBuilder.Create()
                                .QuickStart<Start>(apiKey)
                                .Build();

        bot.Start();

        Console.ReadLine();
    }
}