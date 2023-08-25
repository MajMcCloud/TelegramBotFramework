using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Attributes;
using TelegramBotBase.Base;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Tools;

namespace TelegramBotBase.Form.Navigation;

[DebuggerDisplay("{Index+1} Forms")]
public class NavigationController : FormBase, IStateForm
{
    public NavigationController()
    {
        History = new List<FormBase>();
        Index = -1;
        ForceCleanupOnLastPop = true;

        Init += NavigationController_Init;
        Opened += NavigationController_Opened;
        Closed += NavigationController_Closed;
    }

    public NavigationController(FormBase startForm, params FormBase[] forms) : this()
    {
        Client = startForm.Client;
        Device = startForm.Device;
        startForm.NavigationController = this;

        History.Add(startForm);
        Index = 0;

        if (forms.Length > 0)
        {
            History.AddRange(forms);
            Index = History.Count - 1;
        }
    }

    [SaveState] private List<FormBase> History { get; }

    [SaveState] public int Index { get; set; }

    /// <summary>
    ///     Will replace the controller when poping a form to the root form.
    /// </summary>
    [SaveState]
    public bool ForceCleanupOnLastPop { get; set; }

    /// <summary>
    ///     Returns the current form from the stack.
    /// </summary>
    public FormBase CurrentForm
    {
        get
        {
            if (History.Count == 0)
            {
                return null;
            }

            return History[Index];
        }
    }


    public async Task LoadState(LoadStateEventArgs e)
    {
        if (e.Get("$controller.history.count") == null)
        {
            return;
        }


        var historyCount = e.GetInt("$controller.history.count");

        for (var i = 0; i < historyCount; i++)
        {
            var c = e.GetObject($"$controller.history[{i}]") as Dictionary<string, object>;


            var qname = e.Get($"$controller.history[{i}].type");

            if (qname == null)
            {
                continue;
            }

            var t = Type.GetType(qname);
            if (t == null || !t.IsSubclassOf(typeof(FormBase)))
            {
                continue;
            }

            //No default constructor, fallback
            if (t.GetConstructor(new Type[] { })?.Invoke(new object[] { }) is not FormBase form)
            {
                continue;
            }

            var properties = c.Where(a => a.Key.StartsWith("$"));

            var fields = form.GetType()
                             .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                             .Where(a => a.GetCustomAttributes(typeof(SaveState), true).Length != 0).ToList();

            foreach (var p in properties)
            {
                var f = fields.FirstOrDefault(a => a.Name == p.Key.Substring(1));
                if (f == null)
                {
                    continue;
                }

                try
                {
                    if (f.PropertyType.IsEnum)
                    {
                        var ent = Enum.Parse(f.PropertyType, p.Value.ToString());

                        f.SetValue(form, ent);

                        continue;
                    }


                    f.SetValue(form, p.Value);
                }
                catch (ArgumentException)
                {
                    Conversion.CustomConversionChecks(form, p, f);
                }
                catch
                {
                }
            }

            form.Device = Device;
            form.Client = Client;
            form.NavigationController = this;

            await form.OnInit(new InitEventArgs());

            History.Add(form);
        }
    }

    public Task SaveState(SaveStateEventArgs e)
    {
        e.Set("$controller.history.count", History.Count.ToString());

        var i = 0;
        foreach (var form in History)
        {
            var fields = form.GetType()
                             .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                             .Where(a => a.GetCustomAttributes(typeof(SaveState), true).Length != 0).ToList();

            var dt = new Dictionary<string, object>();
            foreach (var f in fields)
            {
                var val = f.GetValue(form);

                dt.Add("$" + f.Name, val);
            }

            e.Set($"$controller.history[{i}].type", form.GetType().AssemblyQualifiedName);

            e.SetObject($"$controller.history[{i}]", dt);

            i++;
        }

        return Task.CompletedTask;
    }

    private async Task NavigationController_Init(object sender, InitEventArgs e)
    {
        if (CurrentForm == null)
        {
            return;
        }

        await CurrentForm.OnInit(e);
    }

    private async Task NavigationController_Opened(object sender, EventArgs e)
    {
        if (CurrentForm == null)
        {
            return;
        }

        await CurrentForm.OnOpened(e);
    }

    private async Task NavigationController_Closed(object sender, EventArgs e)
    {
        if (CurrentForm == null)
        {
            return;
        }

        await CurrentForm.OnClosed(e);
    }


    /// <summary>
    ///     Remove the current active form on the stack.
    /// </summary>
    /// <returns></returns>
    public virtual async Task PopAsync()
    {
        if (History.Count == 0)
        {
            return;
        }

        var form = History[Index];

        form.NavigationController = null;
        History.Remove(form);
        Index--;

        Device.FormSwitched = true;

        await form.OnClosed(EventArgs.Empty);

        //Leave NavigationController and move to the last one
        if (ForceCleanupOnLastPop && History.Count == 1)
        {
            var lastForm = History[0];
            lastForm.NavigationController = null;
            await NavigateTo(lastForm);
            return;
        }

        if (History.Count > 0)
        {
            form = History[Index];
            await form.OnOpened(EventArgs.Empty);
        }
    }

    /// <summary>
    ///     Pop's through all forms back to the root form.
    /// </summary>
    /// <returns></returns>
    public virtual async Task PopToRootAsync()
    {
        while (Index > 0)
        {
            await PopAsync();
        }
    }

    /// <summary>
    ///     Pushing the given form to the stack and renders it.
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    public virtual async Task PushAsync(FormBase form, params object[] args)
    {
        form.Client = Client;
        form.Device = Device;
        form.NavigationController = this;

        History.Add(form);
        Index++;

        Device.FormSwitched = true;

        if (Index < 2)
        {
            return;
        }

        await form.OnInit(new InitEventArgs(args));

        await form.OnOpened(EventArgs.Empty);
    }

    /// <summary>
    ///     Pops the current form and pushes a new one.
    ///     Will help to remove forms so you can not navigate back to them.
    /// </summary>
    /// <param name="form"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public virtual async Task PushAndReplaceAsync(FormBase form, params object[] args)
    {
        await PopAsync();

        await PushAsync(form, args);
    }

    public List<FormBase> GetAllForms()
    {
        return History.ToList();
    }


    #region "Methods passthrough"

    public override async Task NavigateTo(FormBase newForm, params object[] args)
    {
        await CurrentForm.NavigateTo(newForm, args);
    }

    public override async Task LoadControls(MessageResult message)
    {
        await CurrentForm.LoadControls(message);
    }

    public override async Task Load(MessageResult message)
    {
        await CurrentForm.Load(message);
    }

    public override async Task ActionControls(MessageResult message)
    {
        await CurrentForm.ActionControls(message);
    }

    public override async Task Action(MessageResult message)
    {
        await CurrentForm.Action(message);
    }


    public override async Task Edited(MessageResult message)
    {
        await CurrentForm.Edited(message);
    }

    public override async Task Render(MessageResult message)
    {
        await CurrentForm.Render(message);
    }

    public override async Task RenderControls(MessageResult message)
    {
        await CurrentForm.RenderControls(message);
    }

    public override async Task PreLoad(MessageResult message)
    {
        await CurrentForm.PreLoad(message);
    }

    public override async Task ReturnFromModal(ModalDialog modal)
    {
        await CurrentForm.ReturnFromModal(modal);
    }

    public override async Task SentData(DataResult message)
    {
        await CurrentForm.SentData(message);
    }

    #endregion
}
