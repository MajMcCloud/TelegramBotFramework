using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using static System.Collections.Specialized.BitVector32;

namespace TelegramBotBase.MessageLoops.Extensions;

public static class MiddlewareBaseMessageLoopExtensions
{
    /// <summary>
    ///     Adds middleware to the message loop then returns the message loop
    /// </summary>
    public static MiddlewareBaseMessageLoop Use(this MiddlewareBaseMessageLoop messageLoop, Func<MessageContainer, Func<Task>, Task> middleware)
    {
        messageLoop.AddMiddleware(middleware);

        return messageLoop;
    }

    /// <summary>
    ///     Adds update type validator middleware to the message loop then returns message loop
    /// </summary>
    public static MiddlewareBaseMessageLoop UseValidUpdateTypes(this MiddlewareBaseMessageLoop messageLoop, params UpdateType[] updateTypes)
    {
        messageLoop.Use(async (container, next) =>
        {
            var updateType = container.UpdateResult.RawData.Type;

            if (updateTypes.Contains(updateType))
            {
                await next();
            }
        });

        return messageLoop;
    }

    /// <summary>
    ///     Adds bot commands handler middleware to the message loop then returns message loop
    /// </summary>
    public static MiddlewareBaseMessageLoop UseBotCommands(this MiddlewareBaseMessageLoop messageLoop)
    {
        messageLoop.Use(async (container, next) =>
        {
            var botBase = container.BotBase;
            var messageResult = container.MessageResult;

            if (messageResult.IsFirstHandler &&
                messageResult.IsBotCommand &&
                botBase.IsKnownBotCommand(messageResult.BotCommand))
            {
                var deviceSession = container.DeviceSession;

                var sce =
                    new BotCommandEventArgs(messageResult.BotCommand,
                                            messageResult.BotCommandParameters,
                                            messageResult.Message,
                                            deviceSession.DeviceId,
                                            deviceSession);

                await botBase.OnBotCommand(sce);

                if (sce.Handled)
                {
                    return;
                }
            }

            await next();
        });

        return messageLoop;
    }

    /// <summary>
    ///     Adds forms handler middleware to the message loop then returns message loop
    /// </summary>
    public static MiddlewareBaseMessageLoop UseForms(this MiddlewareBaseMessageLoop messageLoop)
    {
        messageLoop.Use(async (container, next) =>
        {
            var activeForm = container.DeviceSession.ActiveForm;
            var messageResult = container.MessageResult;

            //Pre Loading Event
            await activeForm.PreLoad(messageResult);

            //Send Load event to controls
            await activeForm.LoadControls(messageResult);

            //Loading Event
            await activeForm.Load(messageResult);

            var updateType = container.UpdateResult.RawData.Type;

            //Is Attachment ? (Photo, Audio, Video, Contact, Location, Document) (Ignore Callback Queries)
            if (updateType == UpdateType.Message)
            {
                if ((messageResult.MessageType == MessageType.Contact)
                    | (messageResult.MessageType == MessageType.Document)
                    | (messageResult.MessageType == MessageType.Location)
                    | (messageResult.MessageType == MessageType.Photo)
                    | (messageResult.MessageType == MessageType.Video)
                    | (messageResult.MessageType == MessageType.Audio))
                {
                    var updateResult = container.UpdateResult;

                    await activeForm.SentData(new DataResult(updateResult));
                }
            }

            var deviceSession = container.DeviceSession;

            //Action Event
            if (!deviceSession.FormSwitched && messageResult.IsAction)
            {
                //Send Action event to controls
                await activeForm.ActionControls(messageResult);

                //Send Action event to form itself
                await activeForm.Action(messageResult);

                if (!messageResult.Handled)
                {
                    messageResult.Handled = true;
                    return;
                }
            }

            if (!deviceSession.FormSwitched)
            {
                //Render Event
                await activeForm.RenderControls(messageResult);

                await activeForm.Render(messageResult);
            }

            await next();
        });

        return messageLoop;
    }

    /// <summary>
    ///     Adds forms pre loads handler middleware to the message loop then returns message loop
    /// </summary>
    public static MiddlewareBaseMessageLoop UsePreLoad(this MiddlewareBaseMessageLoop messageLoop)
    {
        messageLoop.Use(async (container, next) =>
        {
            var activeForm = container.DeviceSession.ActiveForm;
            var messageResult = container.MessageResult;

            await activeForm.PreLoad(messageResult);

            await next();
        });

        return messageLoop;
    }

    /// <summary>
    ///     Adds forms loads handler middleware to the message loop then returns message loop
    /// </summary>
    public static MiddlewareBaseMessageLoop UseLoad(this MiddlewareBaseMessageLoop messageLoop)
    {
        messageLoop.Use(async (container, next) =>
        {
            var activeForm = container.DeviceSession.ActiveForm;
            var messageResult = container.MessageResult;

            await activeForm.LoadControls(messageResult);
            await activeForm.Load(messageResult);

            await next();
        });

        return messageLoop;
    }

    /// <summary>
    ///     Adds forms custom attachments handler middleware to the message loop then returns message loop
    /// </summary>
    public static MiddlewareBaseMessageLoop UseAttachments(this MiddlewareBaseMessageLoop messageLoop, params MessageType[] validAttachments)
    {
        messageLoop.Use(async (container, next) =>
        {
            var activeForm = container.DeviceSession.ActiveForm;
            var messageResult = container.MessageResult;
            var updateType = container.UpdateResult.RawData.Type;

            //Is Attachment ? (Photo, Audio, Video, Contact, Location, Document) (Ignore Callback Queries)
            if (updateType == UpdateType.Message)
            {
                if ((messageResult.MessageType == MessageType.Contact)
                | (messageResult.MessageType == MessageType.Document)
                | (messageResult.MessageType == MessageType.Location)
                | (messageResult.MessageType == MessageType.Photo)
                | (messageResult.MessageType == MessageType.Video)
                | (messageResult.MessageType == MessageType.Audio))
                {
                    if (validAttachments.Contains(messageResult.MessageType))
                    {
                        var updateResult = container.UpdateResult;

                        await activeForm.SentData(new DataResult(updateResult));
                    }
                }
            }

            await next();
        });

        return messageLoop;
    }

    /// <summary>
    ///     Adds forms all attachments handler middleware to the message loop then returns message loop
    /// </summary>
    public static MiddlewareBaseMessageLoop UseAllAttachments(this MiddlewareBaseMessageLoop messageLoop)
    {
        messageLoop.Use(async (container, next) =>
        {
            var activeForm = container.DeviceSession.ActiveForm;
            var messageResult = container.MessageResult;
            var updateType = container.UpdateResult.RawData.Type;

            //Is Attachment ? (Photo, Audio, Video, Contact, Location, Document) (Ignore Callback Queries)
            if (updateType == UpdateType.Message)
            {
                if ((messageResult.MessageType == MessageType.Contact)
                    | (messageResult.MessageType == MessageType.Document)
                    | (messageResult.MessageType == MessageType.Location)
                    | (messageResult.MessageType == MessageType.Photo)
                    | (messageResult.MessageType == MessageType.Video)
                    | (messageResult.MessageType == MessageType.Audio))
                {
                    var updateResult = container.UpdateResult;

                    await activeForm.SentData(new DataResult(updateResult));
                }
            }

            await next();
        });

        return messageLoop;
    }

    /// <summary>
    ///     Adds forms actions handler middleware to the message loop then returns message loop
    /// </summary>
    public static MiddlewareBaseMessageLoop UseActions(this MiddlewareBaseMessageLoop messageLoop)
    {
        messageLoop.Use(async (container, next) =>
        {
            var messageResult = container.MessageResult;
            var deviceSession = container.DeviceSession;
            var activeForm = deviceSession.ActiveForm;

            //Action Event
            if (!deviceSession.FormSwitched && messageResult.IsAction)
            {
                //Send Action event to controls
                await activeForm.ActionControls(messageResult);

                //Send Action event to form itself
                await activeForm.Action(messageResult);

                if (!messageResult.Handled)
                {
                    messageResult.Handled = true;
                    return;
                }
            }

            await next();
        });

        return messageLoop;
    }

    /// <summary>
    ///     Adds forms renders handler middleware to the message loop then returns message loop
    /// </summary>
    public static MiddlewareBaseMessageLoop UseRender(this MiddlewareBaseMessageLoop messageLoop)
    {
        messageLoop.Use(async (container, next) =>
        {
            var messageResult = container.MessageResult;
            var deviceSession = container.DeviceSession;
            var activeForm = deviceSession.ActiveForm;

            if (!deviceSession.FormSwitched)
            {
                await activeForm.RenderControls(messageResult);

                await activeForm.Render(messageResult);
            }

            await next();
        });

        return messageLoop;
    }
}
