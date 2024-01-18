using System;
using System.Diagnostics;
using TelegramBotBase.Base;
using TelegramBotBase.DependencyInjection;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Sessions;

namespace DemoBot.ActionManager.Actions
{
    public class StartWithAction : IExternalAction
    {
        public Type FormType { get; }

        public string Value { get; set; }

        public Action<FormBase, String> SetProperty { get; set; }

        String? _lastValue { get; set; }
        

        public StartWithAction(Type formType, string value, Action<FormBase, string> setProperty)
        {
            FormType = formType;
            Value = value;
            SetProperty = setProperty;
        }

        public bool DoesFit(string raw_action)
        {
            if (!raw_action.StartsWith(Value))
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

    public class StartWithAction<TForm> : IExternalAction
       where TForm : FormBase
    {
        public string Value { get; set; }

        public Action<TForm, String> SetProperty { get; set; }

        String? _lastValue { get; set; }

        public StartWithAction(string value, Action<TForm, String> setProperty)
        {
            Value = value;
            SetProperty = setProperty;
        }


        public bool DoesFit(string raw_action)
        {
            if (!raw_action.StartsWith(Value))
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

    public static class StartWithAction_Extensions
    {

        public static void AddStartsWithAction<TForm>(this ExternalActionManager manager, string value, Action<TForm, String> setProperty)
            where TForm : FormBase
        {
            if (!typeof(FormBase).IsAssignableFrom(typeof(TForm)))
            {
                throw new ArgumentException($"{nameof(TForm)} argument must be a {nameof(FormBase)} type");
            }

            manager.Add(new StartWithAction<TForm>(value, setProperty));
        }

        public static void AddStartsWithAction(this ExternalActionManager manager, Type formType, string value, Action<FormBase, String> setProperty)
        {
            if (!typeof(FormBase).IsAssignableFrom(formType))
            {
                throw new ArgumentException($"{nameof(formType)} argument must be a {nameof(FormBase)} type");
            }

            manager.Add(new StartWithAction(formType, value, setProperty));
        }

    }

}
