using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Experiments.ActionManager.Navigation
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


        public async Task DoAction(UpdateResult ur, MessageResult mr)
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


        public async Task DoAction(UpdateResult ur, MessageResult mr)
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
}
