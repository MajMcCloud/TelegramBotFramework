using DemoBot.ActionManager;
using DemoBot.ActionManager.Actions;
using DemoBot.Forms;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotBase.Builder;
using TelegramBotBase.Commands;
using TelegramBotBase.Form;

namespace DemoBot
{
    internal class Program
    {
        public static String Token = Environment.GetEnvironmentVariable("API_KEY") ?? throw new Exception("API_KEY is not set");

        static async Task Main(string[] args)
        {

            //Using a custom FormBase message loop which is based on the original FormBaseMessageLoop within the framework.
            //Would integrate this later into the BotBaseBuilder -> MessageLoop step.
            var cfb = new CustomFormBaseMessageLoop();

            var eam = new ExternalActionManager();

            eam.AddStartsWithAction<HiddenForm>("n_", (a, b) =>
            {
                a.value = b;
            });

            eam.AddStartsWithAction(typeof(HiddenForm), "t_", (a, b) =>
            {
                var hf = a as HiddenForm;
                if (hf == null)
                    return;

                hf.value = b;
            });

            eam.AddGuidAction<HiddenTicketForm>("tickets", (a, b) =>
            {
                a.ticketId = b;
            });

            eam.AddGuidAction<HiddenOpenForm>("open", (a, b) =>
            {
                a.guid = b;
            });

            cfb.ExternalActionManager = eam;

            var bb = BotBaseBuilder.Create()
                                    .WithAPIKey(Token)
                                    .CustomMessageLoop(cfb)
                                    .WithStartForm<Forms.StartForm>()
                                    .NoProxy()
                                    .CustomCommands(a =>
                                    {
                                        a.Start("Starts the bot");
                                        a.Add("test", "Sends a test notification");


                                    })
                                    .NoSerialization()
                                    .UseGerman()
                                    .UseSingleThread()
                                    .Build();

            bb.BotCommand += Bb_BotCommand;

            bb.UnhandledCall += Bb_UnhandledCall;

            await bb.Start();

            await bb.UploadBotCommands();

            Console.WriteLine("Bot started.");


            Console.ReadLine();


            await bb.Stop();


        }

        private static void Bb_UnhandledCall(object? sender, TelegramBotBase.Args.UnhandledCallEventArgs e)
        {

            Console.WriteLine($"Unhandled call: {e.RawData}");

        }

        private static async Task Bb_BotCommand(object sender, TelegramBotBase.Args.BotCommandEventArgs e)
        {

            var current_form = e.Device.ActiveForm;

            switch (e.Command)
            {
                case "/start":

                    //Already on start form
                    if (current_form.GetType() == typeof(Forms.StartForm))
                    {
                        return;
                    }

                    var st = new Forms.StartForm();

                    await current_form.NavigateTo(st);

                    break;

                case "/test":

                    //Send test notification

                    //Test values

                    String max_value = "n_".PadRight(32, '5'); //Starts with

                    String max_value2 = "t_".PadRight(32, '5'); //Starts with

                    Guid test_value = Guid.NewGuid(); //Unhandled caller

                    var callback_guid = GuidAction.GetCallback("open", Guid.NewGuid()); //HiddenOpenForm

                    var callback_tickets = GuidAction.GetCallback("tickets", Guid.NewGuid()); //HiddenTicketForm


                    String message = $"Test notification from 'outside'\n\nTest values are:\n\nTest: {max_value}\nTest2: {max_value2}\nTest (Guid): {test_value.ToString()}\nTest (Callback Guid): {callback_guid.Value}\nTickets (Guid): {callback_tickets.Value}\n";


                    var tb = new TelegramBotClient(Token);

                    var bf = new ButtonForm();

                    bf.AddButtonRow(new ButtonBase("Ok", "n_ok"), new ButtonBase("Later", "n_later"));

                    bf.AddButtonRow("Test", max_value);

                    bf.AddButtonRow("Test2", max_value2);

                    bf.AddButtonRow("Test (Guid)", test_value.ToString());

                    bf.AddButtonRow("Test (Callback Gui)", callback_guid);

                    bf.AddButtonRow("Tickets", callback_tickets);

                    bf.AddButtonRow("Close", "close");

                    await tb.SendTextMessageAsync(e.DeviceId, message, disableNotification: true, replyMarkup: (InlineKeyboardMarkup)bf);

                    break;


            }




        }
    }
}
