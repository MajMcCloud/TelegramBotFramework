using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Telegram.Bot;
using TelegramBotBase.Args;
using TelegramBotBase.Builder;
using TelegramBotBase.Commands;
using TelegramBotBase.Enums;
using TelegramBotBase.Example.Tests;

namespace TelegramBotBase.Example;

internal class Program
{
    public static BotBase bot = null;

    static Tasks.WelcomeTask welcomeTask = null;

    private static async Task Main(string[] args)
    {
        bot = BotBaseBuilder
                 .Create()
                 .WithAPIKey(Environment.GetEnvironmentVariable("API_KEY") ??
                             throw new Exception("API_KEY is not set"))
                 .DefaultMessageLoop()
                 .WithStartForm<Start>()
                 .NoProxy()
                 .UseDefaultRequestDispatcher()
                 .CustomCommands(a =>
                 {
                     a.Start("Starts the bot");
                     a.Add("myid", "Returns my Device ID");
                     a.Help("Should show you some help");
                     a.Settings("Should show you some settings");
                     a.Add("form1", "Opens test form 1");
                     a.Add("form2", "Opens test form 2");
                     a.Add("params", "Returns all send parameters as a message.");
                 })
                 .NoSerialization()
                 .UseEnglish()
                 .UseThreadPool()
                 .Build();


        bot.BotCommand += Bb_BotCommand;

        //Update Bot commands to botfather
        await bot.UploadBotCommands();

        bot.SetSetting(ESettings.LogAllMessages, true);

        bot.Message += (s, en) =>
        {
            Console.WriteLine(en.DeviceId + " " + en.Message.MessageText + " " + (en.Message.RawData ?? ""));
        };

        //Start welcome task
        welcomeTask = new Tasks.WelcomeTask();

        welcomeTask.RunAsync();

        await bot.Start();


        do
        {
            Console.Clear();

            Console.WriteLine("Telegram Bot started...");

            Console.WriteLine("Press w to open bot in browser.");
            Console.WriteLine("Press q to quit application.");

            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.W:
                    var me = await bot.Client.TelegramClient.GetMe();
                    //Open Website in default browser
                    var url = $"https://t.me/{me.Username}";
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                    continue;
                default:

                    await bot.Stop();

                    return;

            }

        } while (true);

    }

    private static async Task Bb_BotCommand(object sender, BotCommandEventArgs en)
    {
        switch (en.Command)
        {
            case "/start":


                var start = new Menu();

                await en.Device.ActiveForm.NavigateTo(start);

                break;
            case "/form1":

                var form1 = new TestForm();

                await en.Device.ActiveForm.NavigateTo(form1);


                break;


            case "/form2":

                var form2 = new TestForm2();

                await en.Device.ActiveForm.NavigateTo(form2);

                break;

            case "/myid":

                await en.Device.Send($"Your Device ID is: {en.DeviceId}");

                en.Handled = true;

                break;

            case "/params":

                var m = en.Parameters.DefaultIfEmpty("").Aggregate((a, b) => a + " and " + b);

                await en.Device.Send("Your parameters are: " + m, replyTo: en.Device.LastMessageId);

                en.Handled = true;

                break;
        }
    }
}
