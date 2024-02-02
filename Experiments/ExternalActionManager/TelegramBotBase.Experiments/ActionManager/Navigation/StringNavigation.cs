using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Experiments.ActionManager.Navigation
{
    public class StringNavigation : IExternalAction
    {
        public Type FormType { get; }

        public string Method { get; set; }

        public Action<FormBase, String> SetProperty { get; set; }

        String? _lastValue { get; set; }

        public StringNavigation(Type formType, string method, Action<FormBase, String> setProperty)
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

            _lastValue = cd.Value;

            return true;
        }


        public async Task DoAction(UpdateResult ur, MessageResult mr)
        {
            await mr.ConfirmAction();

            var new_form = FormType.GetConstructor(new Type[] { })?.Invoke(new object[] { }) as FormBase;

            if (_lastValue != null)
                SetProperty(new_form, _lastValue);

            await ur.Device.ActiveForm.NavigateTo(new_form);
        }


        public static CallbackData GetCallback(string method, String str) => new CallbackData(method, str);
    }

    public class StringNavigation<TForm> : IExternalAction
        where TForm : FormBase
    {
        public string Method { get; set; }

        public Action<TForm, String> SetProperty { get; set; }

        String? _lastValue { get; set; }

        public StringNavigation(string method, Action<TForm, String> setProperty)
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

            _lastValue = cd.Value;

            return true;
        }


        public async Task DoAction(UpdateResult ur, MessageResult mr)
        {
            await mr.ConfirmAction();

            var type = typeof(TForm);

            TForm new_form = type.GetConstructor(new Type[] { })?.Invoke(new object[] { }) as TForm;

            if (_lastValue != null)
                SetProperty(new_form, _lastValue);

            await ur.Device.ActiveForm.NavigateTo(new_form);
        }
    }
}
