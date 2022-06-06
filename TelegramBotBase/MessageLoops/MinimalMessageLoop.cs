using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Enums;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.MessageLoops
{
    /// <summary>
    /// This is a minimal message loop which will react to all update types and just calling the Load method.
    /// </summary>
    public class MinimalMessageLoop : IMessageLoopFactory
    {
        private static object __evUnhandledCall = new object();

        private EventHandlerList __Events = new EventHandlerList();

        public MinimalMessageLoop()
        {

        }

        public async Task MessageLoop(BotBase Bot, DeviceSession session, UpdateResult ur, MessageResult mr)
        {
            var update = ur.RawData;


            mr.Device = session;

            var activeForm = session.ActiveForm;

            //Loading Event
            await activeForm.Load(mr);

        }

        /// <summary>
        /// Will be called if no form handeled this call
        /// </summary>
        public event EventHandler<UnhandledCallEventArgs> UnhandledCall
        {
            add
            {
                this.__Events.AddHandler(__evUnhandledCall, value);
            }
            remove
            {
                this.__Events.RemoveHandler(__evUnhandledCall, value);
            }
        }

        public void OnUnhandledCall(UnhandledCallEventArgs e)
        {
            (this.__Events[__evUnhandledCall] as EventHandler<UnhandledCallEventArgs>)?.Invoke(this, e);

        }
    }
}
