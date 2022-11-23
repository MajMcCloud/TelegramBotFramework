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
    /// This message loop reacts to all update types.
    /// </summary>
    public class FullMessageLoop : IMessageLoopFactory
    {
        private static object __evUnhandledCall = new object();

        private EventHandlerList __Events = new EventHandlerList();

        public FullMessageLoop()
        {

        }

        public async Task MessageLoop(BotBase Bot, DeviceSession session, UpdateResult ur, MessageResult mr)
        {
            var update = ur.RawData;


            //Is this a bot command ?
            if (mr.IsFirstHandler && mr.IsBotCommand && Bot.IsKnownBotCommand(mr.BotCommand))
            {
                var sce = new BotCommandEventArgs(mr.BotCommand, mr.BotCommandParameters, mr.Message, session.DeviceId, session);
                await Bot.OnBotCommand(sce);

                if (sce.Handled)
                    return;
            }

            mr.Device = session;

            var activeForm = session.ActiveForm;

            //Pre Loading Event
            await activeForm.PreLoad(mr);

            //Send Load event to controls
            await activeForm.LoadControls(mr);

            //Loading Event
            await activeForm.Load(mr);


            //Is Attachment ? (Photo, Audio, Video, Contact, Location, Document) (Ignore Callback Queries)
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                if (mr.MessageType == Telegram.Bot.Types.Enums.MessageType.Contact
                    | mr.MessageType == Telegram.Bot.Types.Enums.MessageType.Document
                    | mr.MessageType == Telegram.Bot.Types.Enums.MessageType.Location
                    | mr.MessageType == Telegram.Bot.Types.Enums.MessageType.Photo
                    | mr.MessageType == Telegram.Bot.Types.Enums.MessageType.Video
                    | mr.MessageType == Telegram.Bot.Types.Enums.MessageType.Audio)
                {
                    await activeForm.SentData(new DataResult(ur));
                }
            }

            //Action Event
            if (!session.FormSwitched && mr.IsAction)
            {
                //Send Action event to controls
                await activeForm.ActionControls(mr);

                //Send Action event to form itself
                await activeForm.Action(mr);

                if (!mr.Handled)
                {
                    var uhc = new UnhandledCallEventArgs(ur.Message.Text, mr.RawData, session.DeviceId, mr.MessageId, ur.Message, session);
                    OnUnhandledCall(uhc);

                    if (uhc.Handled)
                    {
                        mr.Handled = true;
                        if (!session.FormSwitched)
                        {
                            return;
                        }
                    }
                }

            }

            if (!session.FormSwitched)
            {
                //Render Event
                await activeForm.RenderControls(mr);

                await activeForm.Render(mr);
            }

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
