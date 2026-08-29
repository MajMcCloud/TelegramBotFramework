using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace TelegramBotBase.Example.Tasks
{
    /// <summary>
    /// Send a welcome message to new users every 5 minutes.
    /// </summary>
    public class WelcomeTask : IDisposable
    {
        Timer _timer;



        public void RunAsync()
        {
            _timer = new Timer(async (state) => await DoStuff(), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));

        }

        async Task DoStuff()
        {
            // Get all users from the database
            var users = Program.bot.Sessions.GetUserSessions();

            var dispatcher = Program.bot.RequestDispatcher;
            foreach (var user in users)
            {

                await dispatcher.Dispatch(user, async (session) =>
                {
                    await session.SendMessage(user.DeviceId, "Welcome to the bot! This is a welcome message sent every 5 minutes.");
                }); 

            }
        }


        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
