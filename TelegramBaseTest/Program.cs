using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase;
using TelegramBotBase.Form;
using TelegramBotBase.Tests;

namespace TelegramBaseTest
{
    class Program
    {
        static void Main(string[] args)
        {

            String APIKey = "";

            BotBase<TestForm> bb = new BotBase<TestForm>(APIKey);

            bb.SystemCalls.Add("/start");
            bb.SystemCalls.Add("/form1");
            bb.SystemCalls.Add("/form2");

            bb.SystemCall += async (s, en) =>
            {
                switch (en.Command)
                {
                    case "/form1":

                        var form1 = new TestForm();
                        await form1.Init();

                        await en.Device.ActiveForm.NavigateTo(form1);

                        break;
                    case "/form2":

                        var form2 = new TestForm2();
                        await form2.Init();

                        await en.Device.ActiveForm.NavigateTo(form2);

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
