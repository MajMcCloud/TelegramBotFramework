using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Form.Navigation;
using TelegramBotBase.Sessions;
using static TelegramBotBase.Base.Async;

namespace TelegramBotBase.Form;

/// <summary>
///     Base class for forms
/// </summary>
public class FormBase : IDisposable
{
    private static readonly object EvInit = new();

    private static readonly object EvOpened = new();

    private static readonly object EvClosed = new();


    public EventHandlerList Events = new();


    public NavigationController NavigationController { get; set; }

    public DeviceSession Device { get; set; }

    public MessageClient Client { get; set; }

    IServiceProvider _serviceProvider = null;

    /// <summary>
    ///     has this formular already been disposed ?
    /// </summary>
    public bool IsDisposed { get; set; }

    public List<ControlBase> Controls { get; set; }

    public FormBase()
    {
        Controls = new List<ControlBase>();
    }

    public FormBase(MessageClient client) : this()
    {
        Client = client;
    }

    /// <summary>
    ///     Cleanup
    /// </summary>
    public void Dispose()
    {
        Client = null;
        Device = null;
        IsDisposed = true;
    }


    public async Task OnInit(InitEventArgs e)
    {
        var handler = Events[EvInit]?.GetInvocationList().Cast<AsyncEventHandler<InitEventArgs>>();
        if (handler == null)
        {
            return;
        }

        foreach (var h in handler)
        {
            await h.InvokeAllAsync(this, e);
        }
    }

    ///// <summary>
    ///// Will get called at the initialization (once per context)
    ///// </summary>
    public event AsyncEventHandler<InitEventArgs> Init
    {
        add => Events.AddHandler(EvInit, value);
        remove => Events.RemoveHandler(EvInit, value);
    }


    public async Task OnOpened(EventArgs e)
    {
        var handler = Events[EvOpened]?.GetInvocationList().Cast<AsyncEventHandler<EventArgs>>();
        if (handler == null)
        {
            return;
        }

        foreach (var h in handler)
        {
            await h.InvokeAllAsync(this, e);
        }
    }

    /// <summary>
    ///     Gets invoked if gets navigated to this form
    /// </summary>
    /// <returns></returns>
    public event AsyncEventHandler<EventArgs> Opened
    {
        add => Events.AddHandler(EvOpened, value);
        remove => Events.RemoveHandler(EvOpened, value);
    }


    public async Task OnClosed(EventArgs e)
    {
        var handler = Events[EvClosed]?.GetInvocationList().Cast<AsyncEventHandler<EventArgs>>();
        if (handler == null)
        {
            return;
        }

        foreach (var h in handler)
        {
            await h.InvokeAllAsync(this, e);
        }
    }


    /// <summary>
    ///     Form has been closed (left)
    /// </summary>
    /// <returns></returns>
    public event AsyncEventHandler<EventArgs> Closed
    {
        add => Events.AddHandler(EvClosed, value);
        remove => Events.RemoveHandler(EvClosed, value);
    }

    /// <summary>
    ///     Get invoked when a modal child from has been closed.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public virtual Task ReturnFromModal(ModalDialog modal)
    {
        return Task.CompletedTask;
    }


    /// <summary>
    ///     Pre to form close, cleanup all controls
    /// </summary>
    /// <returns></returns>
    public Task CloseControls()
    {
        foreach (var b in Controls)
        {
            b.Cleanup().Wait();
        }

        return Task.CompletedTask;
    }

    public virtual Task PreLoad(MessageResult message)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Gets invoked if a message was sent or an action triggered
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public virtual async Task LoadControls(MessageResult message)
    {
        //Looking for the control by id, if not listened, raise event for all
        if (message.RawData?.StartsWith("#c") ?? false)
        {
            var c = Controls.FirstOrDefault(a => a.ControlId == message.RawData.Split('_')[0]);
            if (c != null)
            {
                await c.Load(message);
                return;
            }
        }

        foreach (var b in Controls)
        {
            if (!b.Enabled)
            {
                continue;
            }

            await b.Load(message);
        }
    }

    /// <summary>
    ///     Gets invoked if the form gets loaded and on every message belongs to this context
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public virtual Task Load(MessageResult message)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Gets invoked, when a messages has been edited.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public virtual Task Edited(MessageResult message)
    {
        return Task.CompletedTask;
    }


    /// <summary>
    ///     Gets invoked if the user clicked a button.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public virtual async Task ActionControls(MessageResult message)
    {
        //Looking for the control by id, if not listened, raise event for all
        if (message.RawData.StartsWith("#c"))
        {
            var c = Controls.FirstOrDefault(a => a.ControlId == message.RawData.Split('_')[0]);
            if (c != null)
            {
                await c.Action(message, message.RawData.Split('_')[1]);
                return;
            }
        }

        foreach (var b in Controls)
        {
            if (!b.Enabled)
            {
                continue;
            }

            await b.Action(message);

            if (message.Handled)
            {
                return;
            }
        }
    }

    /// <summary>
    ///     Gets invoked if the user has clicked a button.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public virtual Task Action(MessageResult message)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Gets invoked if the user has sent some media (Photo, Audio, Video, Contact, Location, Document)
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public virtual Task SentData(DataResult message)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Gets invoked at the end of the cycle to "Render" text, images, buttons, etc...
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public virtual async Task RenderControls(MessageResult message)
    {
        foreach (var b in Controls)
        {
            if (!b.Enabled)
            {
                continue;
            }

            await b.Render(message);
        }
    }

    /// <summary>
    ///     Gets invoked at the end of the cycle to "Render" text, images, buttons, etc...
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public virtual Task Render(MessageResult message)
    {
        return Task.CompletedTask;
    }


    /// <summary>
    ///     Navigates to a new form
    /// </summary>
    /// <param name="newForm"></param>
    /// <returns></returns>
    public virtual async Task NavigateTo(FormBase newForm, params object[] args)
    {
        var ds = Device;
        if (ds == null)
        {
            return;
        }

        ds.FormSwitched = true;

        ds.PreviousForm = ds.ActiveForm;

        ds.ActiveForm = newForm;
        newForm.Client = Client;
        newForm.Device = ds;

        //Notify prior to close
        foreach (var b in Controls)
        {
            if (!b.Enabled)
            {
                continue;
            }

            await b.Hidden(true);
        }

        CloseControls().Wait();

        await OnClosed(EventArgs.Empty);

        await newForm.OnInit(new InitEventArgs(args));

        await newForm.OnOpened(EventArgs.Empty);
    }

    /// <summary>
    ///     Opens this form modal, but don't closes the original ones
    /// </summary>
    /// <param name="newForm"></param>
    /// <returns></returns>
    public virtual async Task OpenModal(ModalDialog newForm, params object[] args)
    {
        var ds = Device;
        if (ds == null)
        {
            return;
        }

        var parentForm = this;

        ds.FormSwitched = true;

        ds.PreviousForm = ds.ActiveForm;

        ds.ActiveForm = newForm;
        newForm.Client = parentForm.Client;
        newForm.Device = ds;
        newForm.ParentForm = parentForm;

        newForm.Closed += async (s, en) => { await CloseModal(newForm, parentForm); };

        foreach (var b in Controls)
        {
            if (!b.Enabled)
            {
                continue;
            }

            await b.Hidden(false);
        }

        await newForm.OnInit(new InitEventArgs(args));

        await newForm.OnOpened(EventArgs.Empty);
    }

    public Task CloseModal(ModalDialog modalForm, FormBase oldForm)
    {
        var ds = Device;
        if (ds == null)
        {
            return Task.CompletedTask;
        }

        if (modalForm == null)
        {
            throw new Exception("No modal form");
        }

        ds.FormSwitched = true;

        ds.PreviousForm = ds.ActiveForm;

        ds.ActiveForm = oldForm;
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Adds a control to the formular and sets its ID and Device.
    /// </summary>
    /// <param name="control"></param>
    public void AddControl(ControlBase control)
    {
        //Duplicate check
        if (Controls.Contains(control))
        {
            throw new ArgumentException("Control has been already added.");
        }

        control.Id = Controls.Count + 1;
        control.Device = Device;
        Controls.Add(control);

        control.Init();
    }

    /// <summary>
    ///     Removes control from the formular and runs a cleanup on it.
    /// </summary>
    /// <param name="control"></param>
    public void RemoveControl(ControlBase control)
    {
        if (!Controls.Contains(control))
        {
            return;
        }

        control.Cleanup().Wait();

        Controls.Remove(control);
    }

    /// <summary>
    ///     Removes all controls.
    /// </summary>
    public void RemoveAllControls()
    {
        foreach (var c in Controls)
        {
            c.Cleanup().Wait();

            Controls.Remove(c);
        }
    }

    /// <summary>
    /// Returns if this instance is a subclass of AutoCleanForm. Necessary to prevent message deletion if not necessary.
    /// </summary>
    public bool IsAutoCleanForm() => this.GetType().IsSubclassOf(typeof(AutoCleanForm));

}