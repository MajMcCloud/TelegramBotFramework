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



            BotBase<TestForm> bb = new BotBase<TestForm>("480896099:AAHo0KIurkdugqdZ1tYh7sik3tJ9guH2uuI");

            bb.SystemCalls.Add("/start");
            bb.SystemCalls.Add("/form1");
            bb.SystemCalls.Add("/form2");

            bb.SystemCall += (s, en) =>
            {
                switch(en.Command)
                {
                    case "/form1":

                        var form1 = new TestForm();
                        form1.Init();

                        en.Device.ActiveForm.NavigateTo(form1);

                        break;
                    case "/form2":

                        var form2 = new TestForm2();
                        form2.Init();

                        en.Device.ActiveForm.NavigateTo(form2);

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
