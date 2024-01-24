using System.Linq.Expressions;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace DemoBot.ActionManager.Navigation
{
    public class EndsWithNavigation : IExternalAction
    {
        public Type FormType { get; }

        public string Value { get; set; }

        public Action<FormBase, string> SetProperty { get; set; }

        public EndsWithNavigation(Type formType, string value, Action<FormBase, string> setProperty)
        {
            FormType = formType;
            Value = value;
            SetProperty = setProperty;
        }


        public bool DoesFit(string raw_data) => raw_data.EndsWith(Value);


        public async Task DoAction(String raw_data, UpdateResult ur, MessageResult mr)
        {
            await mr.ConfirmAction();

            var new_form = FormType.GetConstructor(new Type[] { })?.Invoke(new object[] { }) as FormBase;

            if (raw_data != null)
            {
                SetProperty(new_form, raw_data);
            }

            await ur.Device.ActiveForm.NavigateTo(new_form);
        }
    }

    public class EndsWithNavigation<TForm> : IExternalAction
       where TForm : FormBase
    {
        public string Value { get; set; }

        public Action<TForm, string> SetProperty { get; set; }

        public EndsWithNavigation(string value, Action<TForm, string> setProperty)
        {
            Value = value;
            SetProperty = setProperty;
        }


        public bool DoesFit(string raw_data) => raw_data.EndsWith(Value);


        public async Task DoAction(String raw_data, UpdateResult ur, MessageResult mr)
        {
            await mr.ConfirmAction();

            var type = typeof(TForm);

            TForm new_form = type.GetConstructor(new Type[] { })?.Invoke(new object[] { }) as TForm;

            if (raw_data != null)
            {
                SetProperty(new_form, raw_data);
            }

            await ur.Device.ActiveForm.NavigateTo(new_form);
        }



    }

    public static class EndsWithNavigation_Extensions
    {
        public static void AddEndsWithNavigation<TForm>(this ExternalActionManager manager, string method, Expression<Func<TForm, string>> propertySelector)
            where TForm : FormBase
        {
            if (!typeof(FormBase).IsAssignableFrom(typeof(TForm)))
            {
                throw new ArgumentException($"{nameof(TForm)} argument must be a {nameof(FormBase)} type");
            }

            var newValue = Expression.Parameter(propertySelector.Body.Type);

            var assign = Expression.Lambda<Action<TForm, string>>(Expression.Assign(propertySelector.Body, newValue), propertySelector.Parameters[0], newValue);

            var setter = assign.Compile(true);

            manager.Add(new EndsWithNavigation<TForm>(method, setter));
        }
        public static void AddEndsWithNavigation<TForm>(this ExternalActionManager manager, string value, Action<TForm, string> setProperty)
            where TForm : FormBase
        {
            if (!typeof(FormBase).IsAssignableFrom(typeof(TForm)))
            {
                throw new ArgumentException($"{nameof(TForm)} argument must be a {nameof(FormBase)} type");
            }

            manager.Add(new EndsWithNavigation<TForm>(value, setProperty));
        }

        public static void AddEndsWithNavigation(this ExternalActionManager manager, Type formType, string value, Expression<Func<FormBase, string>> propertySelector)
        {
            if (!typeof(FormBase).IsAssignableFrom(formType))
            {
                throw new ArgumentException($"{nameof(formType)} argument must be a {nameof(FormBase)} type");
            }

            var newValue = Expression.Parameter(propertySelector.Body.Type);

            var assign = Expression.Lambda<Action<FormBase, string>>(Expression.Assign(propertySelector.Body, newValue), propertySelector.Parameters[0], newValue);

            var setter = assign.Compile(true);

            manager.Add(new EndsWithNavigation(formType, value, setter));
        }

        public static void AddEndsWithNavigation(this ExternalActionManager manager, Type formType, string value, Action<FormBase, string> setProperty)
        {
            if (!typeof(FormBase).IsAssignableFrom(formType))
            {
                throw new ArgumentException($"{nameof(formType)} argument must be a {nameof(FormBase)} type");
            }

            manager.Add(new EndsWithNavigation(formType, value, setProperty));
        }

    }

}
