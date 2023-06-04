using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Attributes;
using TelegramBotBase.Base;

namespace TelegramBotBase.Form;

[IgnoreState]
public class ConfirmDialog : ModalDialog
{
    public ConfirmDialog()
    {
    }

    public ConfirmDialog(string message)
    {
        Message = message;
        Buttons = new List<ButtonBase>();
    }

    public ConfirmDialog(string message, params ButtonBase[] buttons)
    {
        Message = message;
        Buttons = buttons.ToList();
    }

    /// <summary>
    ///     The message the users sees.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    ///     An additional optional value.
    /// </summary>
    public object Tag { get; set; }

    /// <summary>
    ///     Automatically close form on button click
    /// </summary>
    public bool AutoCloseOnClick { get; set; } = true;

    public List<ButtonBase> Buttons { get; set; }

    private static object EvButtonClicked { get; } = new();

    /// <summary>
    ///     Adds one Button
    /// </summary>
    /// <param name="button"></param>
    public void AddButton(ButtonBase button)
    {
        Buttons.Add(button);
    }

    public override async Task Action(MessageResult message)
    {
        if (message.Handled)
        {
            return;
        }

        if (!message.IsFirstHandler)
        {
            return;
        }

        var call = message.GetData<CallbackData>();
        if (call == null)
        {
            return;
        }

        message.Handled = true;

        await message.ConfirmAction();

        await message.DeleteMessage();

        var button = Buttons.FirstOrDefault(a => a.Value == call.Value);

        if (button == null)
        {
            return;
        }

        OnButtonClicked(new ButtonClickedEventArgs(button) { Tag = Tag });

        if (AutoCloseOnClick)
        {
            await CloseForm();
        }
    }


    public override async Task Render(MessageResult message)
    {
        var btn = new ButtonForm();

        var buttons = Buttons.Select(a => new ButtonBase(a.Text, CallbackData.Create("action", a.Value))).ToList();
        btn.AddButtonRow(buttons);

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