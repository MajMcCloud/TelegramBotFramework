using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Sessions;

namespace DemoBot.ActionManager.Actions
{
    public class GuidAction : IExternalAction
    {
        public Type FormType { get; }

        public String Method { get; set; }

        public Action<FormBase, Guid> SetProperty { get; set; }

        Guid? _lastValue { get; set; }

        public GuidAction(Type formType, string method, Action<FormBase, Guid> setProperty)
        {
            FormType = formType;
            Method = method;
            SetProperty = setProperty;
        }

        public bool DoesFit(string raw_action)
        {
            var cd = CallbackData.Deserialize(raw_action);

            if (cd == null)
                return false;

            if (cd.Method != Method)
                return false;

            Guid g;

            if (Guid.TryParse(cd.Value, out g))
                _lastValue = g;

            return true;
        }


        public async Task DoAction(UpdateResult ur, MessageResult mr, DeviceSession session)
        {
            await mr.ConfirmAction();

            var new_form = FormType.GetConstructor(new Type[] { })?.Invoke(new object[] { }) as FormBase;

            if (_lastValue != null)
                SetProperty(new_form, _lastValue.Value);

            await session.ActiveForm.NavigateTo(new_form);
        }


        public static CallbackData GetCallback(String method, Guid guid)
        {
            return new CallbackData(method, guid.ToString());
        }
    }

    public class GuidAction<TForm> : IExternalAction
        where TForm : FormBase
    {
        public String Method { get; set; }

        public Action<TForm, Guid> SetProperty { get; set; }

        Guid? _lastValue { get; set; }

        public GuidAction(string method, Action<TForm, Guid> setProperty)
        {
            Method = method;
            SetProperty = setProperty;
        }

        public bool DoesFit(string raw_action)
        {
            var cd = CallbackData.Deserialize(raw_action);

            if (cd == null)
                return false;

            if (cd.Method != Method)
                return false;

            Guid g;

            if (Guid.TryParse(cd.Value, out g))
                _lastValue = g;

            return true;
        }


        public async Task DoAction(UpdateResult ur, MessageResult mr, DeviceSession session)
        {
            await mr.ConfirmAction();

            var type = typeof(TForm);

            TForm new_form = type.GetConstructor(new Type[] { })?.Invoke(new object[] { }) as TForm;

            if (_lastValue != null)
                SetProperty(new_form, _lastValue.Value);

            await session.ActiveForm.NavigateTo(new_form);
        }


        public static CallbackData GetCallback(String method, Guid guid)
        {
            return new CallbackData(method, guid.ToString());
        }

    }

    public static class GuidAction_Extensions
    {

        public static void AddGuidAction<TForm>(this ExternalActionManager manager, string method, Expression<Func<TForm, Guid>> propertySelector)
            where TForm : FormBase
        {
            if (!typeof(FormBase).IsAssignableFrom(typeof(TForm)))
            {
                throw new ArgumentException($"{nameof(TForm)} argument must be a {nameof(FormBase)} type");
            }

            var newValue = Expression.Parameter(propertySelector.Body.Type);

            var assign = Expression.Lambda<Action<TForm, Guid>>(Expression.Assign(propertySelector.Body, newValue), propertySelector.Parameters[0], newValue);

            var setter = assign.Compile(true);

            manager.Add(new GuidAction<TForm>(method, setter));
        }

        public static void AddGuidAction<TForm>(this ExternalActionManager manager, string method, Action<TForm, Guid> action)
            where TForm : FormBase
        {
            if (!typeof(FormBase).IsAssignableFrom(typeof(TForm)))
            {
                throw new ArgumentException($"{nameof(TForm)} argument must be a {nameof(FormBase)} type");
            }

            manager.Add(new GuidAction<TForm>(method, action));
        }

        public static void AddGuidAction(this ExternalActionManager manager, Type formType, string method, Action<FormBase, Guid> action)
        {
            if (!typeof(FormBase).IsAssignableFrom(formType))
            {
                throw new ArgumentException($"{nameof(formType)} argument must be a {nameof(FormBase)} type");
            }

            manager.Add(new GuidAction(formType, method, action));
        }
    }
}
