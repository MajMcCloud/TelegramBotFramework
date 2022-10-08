using System;
using System.Timers;
using AsyncFormUpdates.forms;
using TelegramBotBase;
using TelegramBotBase.Builder;

namespace AsyncFormUpdates
{
    internal class Program
    {
        private static BotBase __bot;

        private static void Main(string[] args)
        {
            var apiKey = "APIKey";

            __bot = BotBaseBuilder.Create()
                                .QuickStart<Start>(apiKey)
                                .Build();

            __bot.Start();

            var timer = new Timer(5000);

            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            Console.ReadLine();

            timer.Stop();
            __bot.Stop();
        }

        private static async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            
            foreach(var s in __bot.Sessions.SessionList)
            {
                //Only for AsyncUpdateForm
                if (s.Value.ActiveForm.GetType() != typeof(AsyncFormUpdate) && s.Value.ActiveForm.GetType() != typeof(AsyncFormEdit))
                    continue;

                await __bot.InvokeMessageLoop(s.Key);
            }


        }
    }
}
