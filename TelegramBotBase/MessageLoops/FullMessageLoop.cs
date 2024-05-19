using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Sessions;

namespace TelegramBotBase.MessageLoops;

/// <summary>
///     This message loop reacts to all update types.
/// </summary>
public class FullMessageLoop : IMessageLoopFactory
{
    private static readonly object EvUnhandledCall = new();

    private readonly EventHandlerList _events = new();

    public async Task MessageLoop(BotBase bot, DeviceSession session, UpdateResult ur, MessageResult mr)
    {
        var update = ur.RawData;


        //Is this a bot command ?
        if (mr.IsFirstHandler && mr.IsBotCommand && bot.IsKnownBotCommand(mr.BotCommand))
        {
            var sce = new BotCommandEventArgs(mr.BotCommand, mr.BotCommandParameters, mr.Message, session.DeviceId,
                                              session);
            await bot.OnBotCommand(sce);

            if (sce.Handled)
            {
                return;
            }
        }

        mr.Device = session;
        ur.Device = session;

        var activeForm = session.ActiveForm;

        //Pre Loading Event
        await activeForm.PreLoad(mr);

        //Send Load event to controls
        await activeForm.LoadControls(mr);

        //Loading Event
        await activeForm.Load(mr);


        //Is Attachment ? (Photo, Audio, Video, Contact, Location, Document) (Ignore Callback Queries)
        if (update.Type == UpdateType.Message)
        {
            if ((mr.MessageType == MessageType.Contact)
                | (mr.MessageType == MessageType.Document)
                | (mr.MessageType == MessageType.Location)
                | (mr.MessageType == MessageType.Photo)
                | (mr.MessageType == MessageType.Video)
                | (mr.MessageType == MessageType.Audio))
            {
                await activeForm.SentData(new DataResult(ur));
            }
        }

        //Message edited ?
        if (update.Type == UpdateType.EditedMessage)
        {
            await activeForm.Edited(mr);
        }

        //Action Event
        if (!session.FormSwitched && mr.IsAction && !mr.Handled)
        {
            //Send Action event to controls
            await activeForm.ActionControls(mr);

            if (!mr.Handled)
            {
                //Send Action event to form itself, if not already handled by a control
                await activeForm.Action(mr);
            }

            if (!mr.Handled)
            {
                var uhc = new UnhandledCallEventArgs(ur.Message.Text, mr.RawData, session.DeviceId, mr.MessageId,
                                                     ur.Message, session);
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
    ///     Will be called if no form handled this call
    /// </summary>
    public event EventHandler<UnhandledCallEventArgs> UnhandledCall
    {
        add => _events.AddHandler(EvUnhandledCall, value);
        remove => _events.RemoveHandler(EvUnhandledCall, value);
    }

    public void OnUnhandledCall(UnhandledCallEventArgs e)
    {
        (_events[EvUnhandledCall] as EventHandler<UnhandledCallEventArgs>)?.Invoke(this, e);
    }
}
