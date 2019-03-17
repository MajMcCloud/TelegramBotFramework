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

            String APIKey = "480896099:AAEtq_owUqRH62DR0gYc-ZWRI_TWl8El1YQ";

            BotBase<Start> bb = new BotBase<Start>(APIKey);

            bb.SystemCalls.Add("/start");
            bb.SystemCalls.Add("/form1");
            bb.SystemCalls.Add("/form2");

            bb.SystemCall += async (s, en) =>
            {
                switch (en.Command)
                {
                    case "/form1":

                        var form1 = new TestForm();

                        await en.Device.ActiveForm.NavigateTo(form1);

                        break;
                    case "/form2":

                        var form2 = new TestForm2();

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
