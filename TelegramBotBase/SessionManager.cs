using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Attributes;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Sessions;
namespace TelegramBotBase
{
    /// <summary>
    /// Class for managing all active sessions
    /// </summary>
    public sealed class SessionManager
    {
        /// <summary>
        /// The Basic message client.
        /// </summary>
        public MessageClient Client => BotBase.Client;

        /// <summary>
        /// A list of all active sessions.
        /// </summary>
        public Dictionary<long, DeviceSession> SessionList { get; set; }


        /// <summary>
        /// Reference to the Main BotBase instance for later use.
        /// </summary>
        public BotBase BotBase { get; }


        public SessionManager(BotBase botBase)
        {
            BotBase = botBase;
            SessionList = new Dictionary<long, DeviceSession>();
        }

        /// <summary>
        /// Get device session from Device/ChatId
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public DeviceSession GetSession(long deviceId)
        {
            var ds = SessionList.FirstOrDefault(a => a.Key == deviceId).Value ?? null;
            return ds;
        }

        /// <summary>
        /// Start a new session
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public async Task<DeviceSession> StartSession(long deviceId)
        {
            var start = BotBase.StartFormFactory.CreateForm();

            start.Client = this.Client;

            DeviceSession ds = new Sessions.DeviceSession(deviceId, start);

            start.Device = ds;
            await start.OnInit(new InitEventArgs());

            await start.OnOpened(new EventArgs());

            SessionList[deviceId] = ds;
            return ds;
        }

        /// <summary>
        /// End session
        /// </summary>
        /// <param name="deviceId"></param>
        public void EndSession(long deviceId)
        {
            var d = SessionList[deviceId];
            if (d != null) SessionList.Remove(deviceId);
        }

        /// <summary>
        /// Returns all active User Sessions.
        /// </summary>
        /// <returns></returns>
        public List<DeviceSession> GetUserSessions()
        {
            return SessionList.Where(a => a.Key > 0).Select(a => a.Value).ToList();
        }

        /// <summary>
        /// Returns all active Group Sessions.
        /// </summary>
        /// <returns></returns>
        public List<DeviceSession> GetGroupSessions()
        {
            return SessionList.Where(a => a.Key < 0).Select(a => a.Value).ToList();
        }

        /// <summary>
        /// Loads the previously saved states from the machine.
        /// </summary>
        public async Task LoadSessionStates()
        {
            if (BotBase.StateMachine == null)
            {
                return;
            }

            await LoadSessionStates(BotBase.StateMachine);
        }


        /// <summary>
        /// Loads the previously saved states from the machine.
        /// </summary>
        public async Task LoadSessionStates(IStateMachine statemachine)
        {
            if (statemachine == null)
            {
                throw new ArgumentNullException("StateMachine", "No StateMachine defined. Please set one to property BotBase.StateMachine");
            }

            var container = statemachine.LoadFormStates();

            foreach (var s in container.States)
            {
                Type t = Type.GetType(s.QualifiedName);
                if (t == null || !t.IsSubclassOf(typeof(FormBase)))
                {
                    continue;
                }

                //Key already existing
                if (SessionList.ContainsKey(s.DeviceId))
                    continue;

                var form = t.GetConstructor(new Type[] { })?.Invoke(new object[] { }) as FormBase;

                //No default constructor, fallback
                if (form == null)
                {
                    if (!statemachine.FallbackStateForm.IsSubclassOf(typeof(FormBase)))
                        continue;

                    form = statemachine.FallbackStateForm.GetConstructor(new Type[] { })?.Invoke(new object[] { }) as FormBase;

                    //Fallback failed, due missing default constructor
                    if (form == null)
                        continue;

                }


                if (s.Values != null && s.Values.Count > 0)
                {
                    var properties = s.Values.Where(a => a.Key.StartsWith("$"));
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
                }

                form.Client = Client;
                var device = new DeviceSession(s.DeviceId, form);

                device.ChatTitle = s.ChatTitle;

                SessionList.Add(s.DeviceId, device);

                //Is Subclass of IStateForm
                var iform = form as IStateForm;
                if (iform != null)
                {
                    var ls = new LoadStateEventArgs();
                    ls.Values = s.Values;
                    iform.LoadState(ls);
                }

                try
                {
                    await form.OnInit(new InitEventArgs());

                    await form.OnOpened(new EventArgs());
                }
                catch
                {
                    //Skip on exception
                    SessionList.Remove(s.DeviceId);
                }
            }

        }




        /// <summary>
        /// Saves all open states into the machine.
        /// </summary>
        public async Task SaveSessionStates(IStateMachine statemachine)
        {
            if (statemachine == null)
            {
                throw new ArgumentNullException("StateMachine", "No StateMachine defined. Please set one to property BotBase.StateMachine");
            }

            var states = new List<StateEntry>();

            foreach (var s in SessionList)
            {
                if (s.Value == null)
                {
                    continue;
                }

                var form = s.Value.ActiveForm;

                try
                {
                    var se = new StateEntry();
                    se.DeviceId = s.Key;
                    se.ChatTitle = s.Value.GetChatTitle();
                    se.FormUri = form.GetType().FullName;
                    se.QualifiedName = form.GetType().AssemblyQualifiedName;

                    //Skip classes where IgnoreState attribute is existing
                    if (form.GetType().GetCustomAttributes(typeof(IgnoreState), true).Length != 0)
                    {
                        //Skip this form, when there is no fallback state form
                        if (statemachine.FallbackStateForm == null)
                        {
                            continue;
                        }

                        //Replace form by default State one.
                        se.FormUri = statemachine.FallbackStateForm.FullName;
                        se.QualifiedName = statemachine.FallbackStateForm.AssemblyQualifiedName;
                    }

                    //Is Subclass of IStateForm
                    var iform = form as IStateForm;
                    if (iform != null)
                    {
                        //Loading Session states
                        SaveStateEventArgs ssea = new SaveStateEventArgs();
                        iform.SaveState(ssea);

                        se.Values = ssea.Values;
                    }

                    //Search for public properties with SaveState attribute
                    var fields = form.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).Where(a => a.GetCustomAttributes(typeof(SaveState), true).Length != 0).ToList();

                    foreach (var f in fields)
                    {
                        var val = f.GetValue(form);

                        se.Values.Add("$" + f.Name, val);
                    }

                    states.Add(se);
                }
                catch
                {
                    //Continue on error (skip this form)
                    continue;
                }
            }

            var sc = new StateContainer();
            sc.States = states;

            statemachine.SaveFormStates(new SaveStatesEventArgs(sc));
        }

        /// <summary>
        /// Saves all open states into the machine.
        /// </summary>
        public async Task SaveSessionStates()
        {
            if (BotBase.StateMachine == null)
                return;


            await SaveSessionStates(BotBase.StateMachine);
        }
    }
}
