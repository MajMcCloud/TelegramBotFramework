using Telegram.Bot;
using TelegramBotBase.Builder;
using TelegramBotBase.Commands;

namespace FileWatcher
{
    internal class Program
    {
        public static Model.Config Config { get; set; }

        public static TelegramBotBase.BotBase Bot { get; set; }

        private static MessageBatcher? Batcher { get; set; }

        static void Main(string[] args)
        {

            Config = Model.Config.Load();

            if (Config == null)
            {
                if(File.Exists(Model.Config.DefaultConfigPath))
                {
                    Console.WriteLine("Config file is invalid.");
                    return;
                }
                else
                {
                    Console.WriteLine("No config file found, creating default config.");
                    Config = new Model.Config
                    {
                        APIKey = "",
                        DirectoryToWatch = "",
                        ListenForCommands = false,
                        DeviceIds = new List<long>(),
                        Filter = "*.*",
                        FilesToExclude = new List<string>(),
                        BatchIntervalSeconds = 5,
                    };
                    Config.Save();
                }
            }

            if (string.IsNullOrEmpty(Config.APIKey))
            {
                Console.WriteLine("No API Key set");
            }

            if (string.IsNullOrEmpty(Config.DirectoryToWatch))
            {
                Console.WriteLine("No directory set");
                return;
            }

            if (Config.FilesToExclude.Count > 0)
            {
                Console.WriteLine("Files to exclude: " + string.Join(',', Config.FilesToExclude));
            }

            FileSystemWatcher watcher = null;

            if (string.IsNullOrEmpty(Config.Filter))
            {
                watcher = new FileSystemWatcher(Config.DirectoryToWatch);
                Console.WriteLine("No filter set, using default *.*");
            }
            else
            {
                watcher = new FileSystemWatcher(Config.DirectoryToWatch, Config.Filter);
                Console.WriteLine($"Filter: {Config.Filter}");
            }

            if (Config.ListenForCommands)
            {
                Console.WriteLine("Listening for commands enabled");

            }
            else
            {
                Console.WriteLine("Listening for commands disabled");
            }

            watcher.IncludeSubdirectories = false;

            Console.WriteLine($"Directory: {Config.DirectoryToWatch}");

            //start file watcher even without api key for testing
            if (!string.IsNullOrEmpty(Config.APIKey))
            {
                Bot = BotBaseBuilder.Create()
                    .WithAPIKey(Config.APIKey)
                    .DefaultMessageLoop()
                    .WithStartForm<Forms.Start>()
                    .NoProxy()
                    .CustomCommands(a =>
                    {
                        a.Start("Starts the bot");
                        a.Add("myid", "Whats my id?");

                    })
                    .NoSerialization()
                    .UseGerman()
                    .UseSingleThread()
                    .Build();

                Bot.BotCommand += Bot_BotCommand;

                Bot.UploadBotCommands();

                if (Config.ListenForCommands)
                {
                    Bot.Start();
                }
            }

            watcher.EnableRaisingEvents = true;

            // Initialize the message batcher
            Batcher = new MessageBatcher(Config.BatchIntervalSeconds, SendToAllDevicesAsync);

            if (!string.IsNullOrEmpty(Config.ServerName))
            {
                Console.WriteLine($"Server name: {Config.ServerName}");
            }

            Console.WriteLine($"Batch interval: {Config.BatchIntervalSeconds}s");

            watcher.Created += Watcher_FileEvent;
            watcher.Changed += Watcher_FileEvent;
            watcher.Renamed += Watcher_FileEvent;

            Console.WriteLine("Bot started.");


            Console.ReadLine();

            watcher.EnableRaisingEvents = false;

            if (Config.ListenForCommands)
            {
                Bot?.Stop();
            }
        }



        private static async Task Bot_BotCommand(object sender, TelegramBotBase.Args.BotCommandEventArgs e)
        {
            switch (e.Command)
            {
                case "/myid":

                    await e.Device.Send($"Your ID is: {e.DeviceId}");

                    e.Handled = true;

                    break;


            }
        }

        private static string FormatMessage(string? filename, string action)
        {
            var s = Config.MessageTemplate
                .Replace(Model.Config.FilenamePlaceholder, filename)
                .Replace(Model.Config.ActionPlaceholder, action)
                .Replace(Model.Config.ServerNamePlaceholder, Config.ServerName);

            if (!string.IsNullOrEmpty(Config.ServerName)
                && !Config.MessageTemplate.Contains(Model.Config.ServerNamePlaceholder))
            {
                s = $"[{Config.ServerName}] {s}";
            }

            return s;
        }

        private static async Task SendToAllDevicesAsync(string message)
        {
            if (Bot == null)
                return;

            foreach (var device in Config.DeviceIds)
            {
                try
                {
                    await Bot.Client.TelegramClient.SendTextMessageAsync(device, message);
                }
                catch
                {
                }
            }
        }

        private static void Watcher_FileEvent(object sender, FileSystemEventArgs e)
        {
            if (Config.FilesToExclude.Count > 0)
            {
                var fn = Path.GetFileName(e.Name);

                if (Config.FilesToExclude.Contains(fn))
                    return;
            }
            
            var s = FormatMessage(e.Name, e.ChangeType.ToString());

            Console.WriteLine(s);

            Batcher?.Enqueue(s);
        }
    }
}
