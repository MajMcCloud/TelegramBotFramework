﻿using Telegram.Bot;
using TelegramBotBase.Builder;
using TelegramBotBase.Commands;

namespace FileWatcher
{
    internal class Program
    {
        public static Model.Config Config { get; set; }

        public static TelegramBotBase.BotBase Bot { get; set; }

        static void Main(string[] args)
        {

            Config = Model.Config.Load();

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

                Bot.Start();
            }

            watcher.EnableRaisingEvents = true;

            watcher.Created += Watcher_Created;
            watcher.Changed += Watcher_Changed;
            watcher.Renamed += Watcher_Renamed;

            Console.WriteLine("Bot started.");


            Console.ReadLine();

            watcher.EnableRaisingEvents = false;

            Bot?.Stop();

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

        private static async void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (Config.FilesToExclude.Count > 0)
            {
                var fn = Path.GetFileName(e.Name);

                if (Config.FilesToExclude.Contains(fn))
                    return;
            }


            Console.WriteLine($"File '{e.Name}' changed");

            if (Bot == null)
                return;

            foreach (var device in Config.DeviceIds)
            {
                try
                {
                    await Bot.Client.TelegramClient.SendTextMessageAsync(device, $"File '{e.Name}' changed");
                }
                catch
                {

                }
            }
        }

        private static async void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            if (Config.FilesToExclude.Count > 0)
            {
                var fn = Path.GetFileName(e.Name);

                if (Config.FilesToExclude.Contains(fn))
                    return;
            }

            Console.WriteLine($"File '{e.Name}' created");

            if (Bot == null)
                return;

            foreach (var device in Config.DeviceIds)
            {
                try
                {
                    await Bot.Client.TelegramClient.SendTextMessageAsync(device, $"File '{e.Name}' created");
                }
                catch
                {

                }
            }
        }   

        private static async void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (Config.FilesToExclude.Count > 0)
            {
                var fn = Path.GetFileName(e.Name);

                if (Config.FilesToExclude.Contains(fn))
                    return;
            }

            Console.WriteLine($"File '{e.Name}' renamed");

            if (Bot == null)
                return;

            foreach (var device in Config.DeviceIds)
            {
                try
                {
                    await Bot.Client.TelegramClient.SendTextMessageAsync(device, $"File '{e.Name}' renamed");
                }
                catch
                {

                }

            }
        }
    }
}
