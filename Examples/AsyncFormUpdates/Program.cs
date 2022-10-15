using System.Timers;
using AsyncFormUpdates.Forms;
using TelegramBotBase;
using TelegramBotBase.Builder;
using Timer = System.Timers.Timer;

namespace AsyncFormUpdates;

internal class Program
{
    private static BotBase __bot;

    private static async Task Main(string[] args)
    {
        __bot = BotBaseBuilder.Create()
                              .QuickStart<Start>(Environment.GetEnvironmentVariable("API_KEY") ??
                                                 throw new Exception("API_KEY is not set"))
                              .Build();

        await __bot.Start();

        var timer = new Timer(5000);

        timer.Elapsed += Timer_Elapsed;
        timer.Start();

        Console.ReadLine();

        timer.Stop();
        await __bot.Stop();
    }

    private static async void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        foreach (var s in __bot.Sessions.SessionList)
        {
            //Only for AsyncUpdateForm
            if (s.Value.ActiveForm.GetType() != typeof(AsyncFormUpdate) &&
                s.Value.ActiveForm.GetType() != typeof(AsyncFormEdit))
            {
                continue;
            }

            await __bot.InvokeMessageLoop(s.Key);
        }
    }
}
