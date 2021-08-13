using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Attributes;
using TelegramBotBase.Base;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Form.Navigation
{
    [DebuggerDisplay("{Index+1} Forms")]
    public class NavigationController : FormBase, IStateForm
    {

        [SaveState]
        private List<FormBase> History { get; set; }

        [SaveState]
        public int Index { get; set; }

        /// <summary>
        /// Will replace the controller when poping a form to the root form.
        /// </summary>
        [SaveState]
        public bool ForceCleanupOnLastPop { get; set; }

        public NavigationController()
        {
            History = new List<FormBase>();
            Index = -1;
            ForceCleanupOnLastPop = true;

            this.Init += NavigationController_Init;
            this.Opened += NavigationController_Opened;
            this.Closed += NavigationController_Closed;
        }

        public NavigationController(FormBase startForm, params FormBase[] forms) : this()
        {
            this.Client = startForm.Client;
            this.Device = startForm.Device;
            startForm.NavigationController = this;

            History.Add(startForm);
            Index = 0;

            if (forms.Length > 0)
            {
                History.AddRange(forms);
                Index = History.Count - 1;
            }
        }

        private async Task NavigationController_Init(object sender, Args.InitEventArgs e)
        {
            if (CurrentForm == null)
                return;

            await CurrentForm.OnInit(e);
        }

        private async Task NavigationController_Opened(object sender, EventArgs e)
        {
            if (CurrentForm == null)
                return;

            await CurrentForm.OnOpened(e);
        }

        private async Task NavigationController_Closed(object sender, EventArgs e)
        {
            if (CurrentForm == null)
                return;

            await CurrentForm.OnClosed(e);
        }



        /// <summary>
        /// Remove the current active form on the stack.
        /// </summary>
        /// <returns></returns>
        public virtual async Task PopAsync()
        {
            if (History.Count == 0)
                return;

            var form = History[Index];

            form.NavigationController = null;
            History.Remove(form);
            Index--;

            Device.FormSwitched = true;

            await form.OnClosed(new EventArgs());

            //Leave NavigationController and move to the last one
            if (ForceCleanupOnLastPop && History.Count == 1)
            {
                var last_form = History[0];
                last_form.NavigationController = null;
                await this.NavigateTo(last_form);
                return;
            }

            if (History.Count > 0)
            {
                form = History[Index];
                await form.OnOpened(new EventArgs());
            }
        }

        /// <summary>
        /// Pop's through all forms back to the root form.
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
        /// Pushing the given form to the stack and renders it.
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public virtual async Task PushAsync(FormBase form, params object[] args)
        {
            form.Client = this.Client;
            form.Device = this.Device;
            form.NavigationController = this;

            this.History.Add(form);
            Index++;

            Device.FormSwitched = true;

            if (Index < 2)
                return;

            await form.OnInit(new InitEventArgs(args));

            await form.OnOpened(new EventArgs());
        }

        /// <summary>
        /// Pops the current form and pushes a new one.
        /// Will help to remove forms so you can not navigate back to them.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual async Task PushAndReplaceAsync(FormBase form, params object[] args)
        {
            await PopAsync();

            await PushAsync(form, args);
        }

        /// <summary>
        /// Returns the current form from the stack.
        /// </summary>
        public FormBase CurrentForm
        {
            get
            {
                if (this.History.Count == 0)
                    return null;

                return this.History[Index];
            }
        }

        public List<FormBase> GetAllForms()
        {
            return History.ToList();
        }


        public void LoadState(LoadStateEventArgs e)
        {
            if (e.Get("$controller.history.count") == null)
                return;


            int historyCount = e.GetInt("$controller.history.count");

            for (int i = 0; i < historyCount; i++)
            {

                var c = e.GetObject($"$controller.history[{i}]") as Dictionary<String, object>;



                var qname = e.Get($"$controller.history[{i}].type");

                if (qname == null)
                    continue;

                Type t = Type.GetType(qname.ToString());
                if (t == null || !t.IsSubclassOf(typeof(FormBase)))
                {
                    continue;
                }

                var form = t.GetConstructor(new Type[] { })?.Invoke(new object[] { }) as FormBase;

                //No default constructor, fallback
                if (form == null)
                {
                    continue;
                }

                var properties = c.Where(a => a.Key.StartsWith("$"));

                var fields = form.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).Where(a => a.GetCustomAttributes(typeof(SaveState), true).Length != 0).ToList();

                foreach (var p in properties)
                {
                    var f = fields.FirstOrDefault(a => a.Name == p.Key.Substring(1));
                    if (f == null)
                        continue;

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
                    catch (ArgumentException ex)
                    {

                        Tools.Conversion.CustomConversionChecks(form, p, f);

                    }
                    catch
                    {

                    }
                }

                form.Device = this.Device;
                form.Client = this.Client;
                form.NavigationController = this;

                form.OnInit(new InitEventArgs());

                this.History.Add(form);
            }

        }

        public void SaveState(SaveStateEventArgs e)
        {
            e.Set("$controller.history.count", History.Count.ToString());

            int i = 0;
            foreach (var form in History)
            {
                var fields = form.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).Where(a => a.GetCustomAttributes(typeof(SaveState), true).Length != 0).ToList();

                var dt = new Dictionary<String, object>();
                foreach (var f in fields)
                {
                    var val = f.GetValue(form);

                    dt.Add("$" + f.Name, val);
                }

                e.Set($"$controller.history[{i}].type", form.GetType().AssemblyQualifiedName);

                e.SetObject($"$controller.history[{i}]", dt);

                i++;
            }
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
}
