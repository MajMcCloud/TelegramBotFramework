using System.Linq.Expressions;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace DemoBot.ActionManager.Navigation
{
    public class GuidNavigation : IExternalAction
    {
        public Type FormType { get; }

        public string Method { get; set; }

        public Action<FormBase, Guid> SetProperty { get; set; }

        Guid? _lastValue { get; set; }

        public GuidNavigation(Type formType, string method, Action<FormBase, Guid> setProperty)
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


        public async Task DoAction(String data, UpdateResult ur, MessageResult mr)
        {
            await mr.ConfirmAction();

            var new_form = FormType.GetConstructor(new Type[] { })?.Invoke(new object[] { }) as FormBase;

            if (_lastValue != null)
                SetProperty(new_form, _lastValue.Value);

            await ur.Device.ActiveForm.NavigateTo(new_form);
        }


        public static CallbackData GetCallback(string method, Guid guid) => new CallbackData(method, guid.ToString());
    }

    public class GuidNavigation<TForm> : IExternalAction
        where TForm : FormBase
    {
        public string Method { get; set; }

        public Action<TForm, Guid> SetProperty { get; set; }

        Guid? _lastValue { get; set; }

        public GuidNavigation(string method, Action<TForm, Guid> setProperty)
        {
            Method = method;
            SetProperty = setProperty;
        }

        public bool DoesFit(string raw_data)
        {
            var cd = CallbackData.Deserialize(raw_data);

            if (cd == null)
                return false;

            if (cd.Method != Method)
                return false;

            Guid g;

            if (Guid.TryParse(cd.Value, out g))
                _lastValue = g;

            return true;
        }


        public async Task DoAction(String raw_data, UpdateResult ur, MessageResult mr)
        {
            await mr.ConfirmAction();

            var type = typeof(TForm);

            TForm new_form = type.GetConstructor(new Type[] { })?.Invoke(new object[] { }) as TForm;

            if (_lastValue != null)
                SetProperty(new_form, _lastValue.Value);

            await ur.Device.ActiveForm.NavigateTo(new_form);
        }


        public static CallbackData GetCallback(string method, Guid guid) => new CallbackData(method, guid.ToString());
    }

    public static class GuidNavigation_Extensions
    {

        public static void AddGuidNavigation<TForm>(this ExternalActionManager manager, string method, Expression<Func<TForm, Guid>> propertySelector)
            where TForm : FormBase
        {
            if (!typeof(FormBase).IsAssignableFrom(typeof(TForm)))
            {
                throw new ArgumentException($"{nameof(TForm)} argument must be a {nameof(FormBase)} type");
            }

            var newValue = Expression.Parameter(propertySelector.Body.Type);

            var assign = Expression.Lambda<Action<TForm, Guid>>(Expression.Assign(propertySelector.Body, newValue), propertySelector.Parameters[0], newValue);

            var setter = assign.Compile(true);

            manager.Add(new GuidNavigation<TForm>(method, setter));
        }

        public static void AddGuidNavigation<TForm>(this ExternalActionManager manager, string method, Action<TForm, Guid> action)
            where TForm : FormBase
        {
            if (!typeof(FormBase).IsAssignableFrom(typeof(TForm)))
            {
                throw new ArgumentException($"{nameof(TForm)} argument must be a {nameof(FormBase)} type");
            }

            manager.Add(new GuidNavigation<TForm>(method, action));
        }

        public static void AddGuidNavigation(this ExternalActionManager manager, Type formType, string value, Expression<Func<FormBase, Guid>> propertySelector)
        {
            if (!typeof(FormBase).IsAssignableFrom(formType))
            {
                throw new ArgumentException($"{nameof(formType)} argument must be a {nameof(FormBase)} type");
            }

            var newValue = Expression.Parameter(propertySelector.Body.Type);

            var assign = Expression.Lambda<Action<FormBase, Guid>>(Expression.Assign(propertySelector.Body, newValue), propertySelector.Parameters[0], newValue);

            var setter = assign.Compile(true);

            manager.Add(new GuidNavigation(formType, value, setter));
        }

        public static void AddGuidNavigation(this ExternalActionManager manager, Type formType, string method, Action<FormBase, Guid> action)
        {
            if (!typeof(FormBase).IsAssignableFrom(formType))
            {
                throw new ArgumentException($"{nameof(formType)} argument must be a {nameof(FormBase)} type");
            }

            manager.Add(new GuidNavigation(formType, method, action));
        }
    }
}
