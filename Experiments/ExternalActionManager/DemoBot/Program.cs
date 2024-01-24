using DemoBot.ActionManager;
using DemoBot.ActionManager.Actions;
using DemoBot.Extensions;
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
            //var cfb = new CustomFormBaseMessageLoop();

            //Example 1: Long version
            var eam = ExternalActionManager.Configure(config =>
            {
                //Waiting for input starting with 'n_'
                config.AddStartsWithAction<HiddenForm>("n_", (a, b) =>
                {
                    a.value = b;
                });


                //Minimal version, using reflection right now
                config.AddStartsWithAction<HiddenForm>("a_", (a, b) =>
                {
                    a.value = b;
                });


                //Waiting for input starting with 't_'
                config.AddStartsWithAction(typeof(HiddenForm), "t_", (a, b) =>
                {
                    var hf = a as HiddenForm;
                    if (hf == null)
                        return;

                    hf.value = b;
                });

                //Minimal version, using reflection right now
                config.AddEndsWithAction<HiddenForm_EndsWith>("_u", (a, b) =>
                {
                    a.value = b;
                });


                //Deserialize input and waiting for the method property to has value 'tickets'
                config.AddGuidAction<HiddenTicketForm>("tickets", (a, b) =>
                {
                    a.ticketId = b;
                });


                //Minimal version, using reflection right now
                config.AddGuidAction<HiddenLetterForm>("letters", (a, b) =>
                {
                    a.letterId = b;
                });


                //Deserialize input and waiting for the method property to has value 'open'
                config.AddGuidAction<HiddenOpenForm>("open", (a, b) =>
                {
                    a.guid = b;
                });

            });

            //Example 2: Short version
            var eam2 = ExternalActionManager.Configure(config =>
            {
                //Waiting for input starting with 'n_'
                config.AddStartsWithAction<HiddenForm>("n_", a => a.value);


                //Waiting for input starting with 'a_'
                config.AddStartsWithAction<HiddenForm>("a_", a => a.value);

                //Waiting for input starting with 't_' (Non generic version)
                config.AddStartsWithAction(typeof(HiddenForm), "t_", a => ((HiddenForm)a).value);

                //Waiting for input ending with 'n_'
                config.AddEndsWithAction<HiddenForm_EndsWith>("_u", a => a.value);


                //Deserialize input and waiting for the method property to has value 'tickets'
                config.AddGuidAction<HiddenTicketForm>("tickets", a => a.ticketId);


                //Minimal version, using reflection right now
                config.AddGuidAction<HiddenLetterForm>("letters", a => a.letterId);


                //Deserialize input and waiting for the method property to has value 'open'
                config.AddGuidAction<HiddenOpenForm>("open", a => a.guid);

            });


            var bb = BotBaseBuilder.Create()
                                    .WithAPIKey(Token)
                                    .ActionMessageLoop(eam2) //.CustomMessageLoop(cfb)
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

                    String max_value3 = "a_".PadRight(32, '5'); //Starts with

                    String max_value4 = "_u".PadLeft(32, '5'); //Ends with

                    Guid test_value = Guid.NewGuid(); //Unhandled caller

                    var callback_guid = GuidAction.GetCallback("open", Guid.NewGuid()); //HiddenOpenForm

                    var callback_tickets = GuidAction.GetCallback("tickets", Guid.NewGuid()); //HiddenTicketForm

                    var callback_letters = GuidAction.GetCallback("letters", Guid.NewGuid()); //HiddenLetterForm


                    String message = $"Test notification from 'outside'\n\nTest values are:\n\nTest: {max_value}\nTest2: {max_value2}\nTest3: {max_value3}\nTest (Guid): {test_value.ToString()}\nTest (Callback Guid): {callback_guid.Value}\nTickets (Guid): {callback_tickets.Value}\nLetters (Guid): {callback_letters.Value}\n";


                    var tb = new TelegramBotClient(Token);

                    var bf = new ButtonForm();

                    bf.AddButtonRow(new ButtonBase("Ok", "n_ok"), new ButtonBase("Later", "n_later"));

                    bf.AddButtonRow("Test", max_value);

                    bf.AddButtonRow("Test2", max_value2);

                    bf.AddButtonRow("Test3", max_value3);

                    bf.AddButtonRow("Test4", max_value4);

                    bf.AddButtonRow("Test (Guid)", test_value.ToString());

                    bf.AddButtonRow("Test (Callback Gui)", callback_guid);

                    bf.AddButtonRow("Tickets", callback_tickets);

                    bf.AddButtonRow("Letters", callback_letters);

                    bf.AddButtonRow("Close", "close");

                    await tb.SendTextMessageAsync(e.DeviceId, message, disableNotification: true, replyMarkup: (InlineKeyboardMarkup)bf);

                    break;


            }




        }
    }
}
