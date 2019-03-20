using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase;
using TelegramBotBase.Form;
using TelegramBaseTest.Tests;

namespace TelegramBaseTest
{
    class Program
    {
        static void Main(string[] args)
        {

            String APIKey = "{YOUR API KEY}";

            BotBase<Start> bb = new BotBase<Start>(APIKey);

            bb.SystemCalls.Add("/start");
            bb.SystemCalls.Add("/form1");
            bb.SystemCalls.Add("/form2");
            bb.SystemCalls.Add("/params");

            bb.SystemCall += async (s, en) =>
            {
                switch (en.Command)
                {
                    case "/start":

                        var start = new Start();

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

            bb.Start();



            Console.WriteLine("Telegram Bot started...");

            Console.WriteLine("Press q to quit application.");


            Console.ReadLine();

            bb.Stop();

        }
    }
}
