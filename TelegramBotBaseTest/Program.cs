using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBotBase;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBaseTest.Tests;
using TelegramBotBase.Commands;
namespace TelegramBotBaseTest
{
    class Program
    {
        static void Main(string[] args)
        {

            BotBase bb = new BotBase(APIKey, new SimpleStartFormFactory(typeof(Start)));

            bb.BotCommands.AddStartCommand("Starts the bot");
            bb.BotCommands.AddHelpCommand("Should show you some help");
            bb.BotCommands.AddSettingsCommand("Should show you some settings");
            bb.BotCommands.Add(new BotCommand() { Command = "form1", Description = "Opens test form 1" });
            bb.BotCommands.Add(new BotCommand() { Command = "form2", Description = "Opens test form 2" });
            bb.BotCommands.Add(new BotCommand() { Command = "params", Description = "Returns all send parameters as a message." });

            bb.BotCommand += async (s, en) =>
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

                    case "/params":

                        String m = en.Parameters.DefaultIfEmpty("").Aggregate((a, b) => a + " and " + b);
                        
                        await en.Device.Send("Your parameters are: " + m, replyTo: en.Device.LastMessageId);

                        en.Handled = true;

                        break;
                }

            };

            //Update Bot commands to botfather
            bb.UploadBotCommands().Wait();

            bb.SetSetting(TelegramBotBase.Enums.eSettings.LogAllMessages, true);

            bb.Message += (s,en) =>
            {
                Console.WriteLine(en.DeviceId + " " + en.Message.MessageText + " " + (en.Message.RawData ?? ""));
            };

            bb.Start();



            Console.WriteLine("Telegram Bot started...");

            Console.WriteLine("Press q to quit application.");


            Console.ReadLine();

            bb.Stop();

        }


    }
}
