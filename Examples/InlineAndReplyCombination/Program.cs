using InlineAndReplyCombination.Forms;
using TelegramBotBase;
using TelegramBotBase.Builder;

namespace InlineAndReplyCombination
{
    internal class Program
    {
        public static BotBase? BotBaseInstance { get; private set; }

        static async Task Main(string[] args)
        {


            BotBaseInstance = BotBaseBuilder.Create()
                    .WithAPIKey(Environment.GetEnvironmentVariable("API_KEY") ??
                        throw new Exception("API_KEY is not set"))
                    .DefaultMessageLoop()
                    .WithStartForm<StartForm>()
                    .NoProxy()
                    .DefaultCommands()
                    .UseJSON(Path.Combine(Directory.GetCurrentDirectory(), "states.json"))
                    .UseEnglish()
                    .Build();



            await BotBaseInstance.UploadBotCommands();

            BotBaseInstance.BotCommand += BotBaseInstance_BotCommand;


            await BotBaseInstance.Start();

            Console.WriteLine("Telegram Bot started");

            Console.ReadLine();


            await BotBaseInstance.Stop();
        }

        private static async Task BotBaseInstance_BotCommand(object sender, TelegramBotBase.Args.BotCommandEventArgs e)
        {
            
            switch(e.Command)
            {
                case "/start":


                    var start = new StartForm();

                    await e.Device.ActiveForm.NavigateTo(start);


                    break;



            }



        }
    }
}