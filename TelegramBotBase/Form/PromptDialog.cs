using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotBase.Args;
using TelegramBotBase.Attributes;
using TelegramBotBase.Base;
using TelegramBotBase.Localizations;
using static TelegramBotBase.Base.Async;

namespace TelegramBotBase.Form;

[IgnoreState]
public class PromptDialog : ModalDialog
{
    public PromptDialog()
    {
    }

    public PromptDialog(string message)
    {
        Message = message;
    }

    /// <summary>
    ///     The message the users sees.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    ///     The returned text value by the user.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    ///     An additional optional value.
    /// </summary>
    public object Tag { get; set; }

    private static object EvCompleted { get; } = new();

    public bool ShowBackButton { get; set; } = false;

    public string BackLabel { get; set; } = Default.Language["PromptDialog_Back"];

    /// <summary>
    ///     Contains the RAW received message.
    /// </summary>
    public Message ReceivedMessage { get; set; }

    public override async Task Load(MessageResult message)
    {
        if (message.Handled)
        {
            return;
        }

        if (!message.IsFirstHandler)
        {
            return;
        }

        if (ShowBackButton && message.MessageText == BackLabel)
        {
            await CloseForm();

            return;
        }

        if (Value == null)
        {
            Value = message.MessageText;

            ReceivedMessage = message.Message;
        }
    }

    public override async Task Render(MessageResult message)
    {
        if (Value == null)
        {
            if (ShowBackButton)
            {
                var bf = new ButtonForm();
                bf.AddButtonRow(new ButtonBase(BackLabel, "back"));
                await Device.Send(Message, (InlineKeyboardMarkup)bf);
                return;
            }

            await Device.Send(Message);
            return;
        }


        message.Handled = true;

        await OnCompleted(new PromptDialogCompletedEventArgs { Tag = Tag, Value = Value });

        if (IsDisplayedAsModal)
            await CloseForm();
    }


    public event AsyncEventHandler<PromptDialogCompletedEventArgs> Completed
    {
        add => Events.AddHandler(EvCompleted, value);
        remove => Events.RemoveHandler(EvCompleted, value);
    }

    public async Task OnCompleted(PromptDialogCompletedEventArgs e)
    {
        var handler = Events[EvCompleted] as AsyncEventHandler<PromptDialogCompletedEventArgs>;
        if (handler != null)
        {
            await handler.Invoke(this, e);
        }
    }
}