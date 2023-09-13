using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Attributes;
using TelegramBotBase.Base;

namespace TelegramBotBase.Form;

/// <summary>
///     A prompt with a lot of buttons
/// </summary>
[IgnoreState]
public class ArrayPromptDialog : FormBase
{
    public ArrayPromptDialog()
    {
    }

    public ArrayPromptDialog(string message)
    {
        Message = message;
    }

    public ArrayPromptDialog(string message, params ButtonBase[][] buttons)
    {
        Message = message;
        Buttons = buttons;
    }

    /// <summary>
    ///     The message the users sees.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    ///     An additional optional value.
    /// </summary>
    public object Tag { get; set; }

    public ButtonBase[][] Buttons { get; set; }

    private static object EvButtonClicked { get; } = new();

    public override async Task Action(MessageResult message)
    {
        var call = message.GetData<CallbackData>();

        message.Handled = true;

        if (!message.IsAction)
        {
            return;
        }

        await message.ConfirmAction();

        await message.DeleteMessage();

        var buttons = Buttons.Aggregate((a, b) => a.Union(b).ToArray()).ToList();

        if (call == null)
        {
            return;
        }

        var button = buttons.FirstOrDefault(a => a.Value == call.Value);

        if (button == null)
        {
            return;
        }

        OnButtonClicked(new ButtonClickedEventArgs(button) { Tag = Tag });
    }


    public override async Task Render(MessageResult message)
    {
        var btn = new ButtonForm();

        foreach (var bl in Buttons)
        {
            btn.AddButtonRow(
                bl.Select(a => new ButtonBase(a.Text, CallbackData.Create("action", a.Value))).ToList());
        }

        await Device.Send(Message, btn);
    }


    public event EventHandler<ButtonClickedEventArgs> ButtonClicked
    {
        add => Events.AddHandler(EvButtonClicked, value);
        remove => Events.RemoveHandler(EvButtonClicked, value);
    }

    public void OnButtonClicked(ButtonClickedEventArgs e)
    {
        (Events[EvButtonClicked] as EventHandler<ButtonClickedEventArgs>)?.Invoke(this, e);
    }
}
