using System;
using System.Diagnostics;
using System.Linq.Expressions;
using TelegramBotBase.Base;
using TelegramBotBase.DependencyInjection;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Sessions;

namespace DemoBot.ActionManager.Actions
{
    public class EndsWithAction : IExternalAction
    {
        public Type FormType { get; }

        public string Value { get; set; }

        public Action<FormBase, String> SetProperty { get; set; }

        String? _lastValue { get; set; }
        

        public EndsWithAction(Type formType, string value, Action<FormBase, string> setProperty)
        {
            FormType = formType;
            Value = value;
            SetProperty = setProperty;
        }

        public bool DoesFit(string raw_action)
        {
            if (!raw_action.EndsWith(Value))
                return false;

            _lastValue = raw_action;

            return true;
        }


        public async Task DoAction(UpdateResult ur, MessageResult mr, DeviceSession session)
        {
            await mr.ConfirmAction();

            var new_form = FormType.GetConstructor(new Type[] { })?.Invoke(new object[] { }) as FormBase;

            if (_lastValue != null)
            {
                SetProperty(new_form, _lastValue);
            }

            await session.ActiveForm.NavigateTo(new_form);
        }
    }

    public class EndsWithAction<TForm> : IExternalAction
       where TForm : FormBase
    {
        public string Value { get; set; }

        public Action<TForm, String> SetProperty { get; set; }

        String? _lastValue { get; set; }

        public EndsWithAction(string value, Action<TForm, String> setProperty)
        {
            Value = value;
            SetProperty = setProperty;
        }


        public bool DoesFit(string raw_action)
        {
            if (!raw_action.EndsWith(Value))
                return false;

            _lastValue = raw_action;

            return true;
        }


        public async Task DoAction(UpdateResult ur, MessageResult mr, DeviceSession session)
        {
            await mr.ConfirmAction();

            var type = typeof(TForm);

            TForm new_form = type.GetConstructor(new Type[] { })?.Invoke(new object[] { }) as TForm;

            if (_lastValue != null)
            {
                SetProperty(new_form, _lastValue);
            }

            await session.ActiveForm.NavigateTo(new_form);
        }



    }

    public static class EndsWithAction_Extensions
    {
        public static void AddEndsWithAction<TForm>(this ExternalActionManager manager, string method, Expression<Func<TForm, String>> propertySelector)
            where TForm : FormBase
        {
            if (!typeof(FormBase).IsAssignableFrom(typeof(TForm)))
            {
                throw new ArgumentException($"{nameof(TForm)} argument must be a {nameof(FormBase)} type");
            }

            var newValue = Expression.Parameter(propertySelector.Body.Type);

            var assign = Expression.Lambda<Action<TForm, String>>(Expression.Assign(propertySelector.Body, newValue), propertySelector.Parameters[0], newValue);

            var setter = assign.Compile(true);

            manager.Add(new EndsWithAction<TForm>(method, setter));
        }
        public static void AddEndsWithAction<TForm>(this ExternalActionManager manager, string value, Action<TForm, String> setProperty)
            where TForm : FormBase
        {
            if (!typeof(FormBase).IsAssignableFrom(typeof(TForm)))
            {
                throw new ArgumentException($"{nameof(TForm)} argument must be a {nameof(FormBase)} type");
            }

            manager.Add(new EndsWithAction<TForm>(value, setProperty));
        }

        public static void AddEndsWithAction(this ExternalActionManager manager, Type formType, string value, Expression<Func<FormBase, String>> propertySelector)
        {
            if (!typeof(FormBase).IsAssignableFrom(formType))
            {
                throw new ArgumentException($"{nameof(formType)} argument must be a {nameof(FormBase)} type");
            }

            var newValue = Expression.Parameter(propertySelector.Body.Type);

            var assign = Expression.Lambda<Action<FormBase, String>>(Expression.Assign(propertySelector.Body, newValue), propertySelector.Parameters[0], newValue);

            var setter = assign.Compile(true);

            manager.Add(new EndsWithAction(formType, value, setter));
        }

        public static void AddEndsWithAction(this ExternalActionManager manager, Type formType, string value, Action<FormBase, String> setProperty)
        {
            if (!typeof(FormBase).IsAssignableFrom(formType))
            {
                throw new ArgumentException($"{nameof(formType)} argument must be a {nameof(FormBase)} type");
            }

            manager.Add(new EndsWithAction(formType, value, setProperty));
        }

    }

}
