using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Experiments.ActionManager.Navigation
{
    public class Int32Navigation : IExternalAction
    {
        public Type FormType { get; }

        public string Method { get; set; }

        public Action<FormBase, int> SetProperty { get; set; }

        int? _lastValue { get; set; }

        public Int32Navigation(Type formType, string method, Action<FormBase, int> setProperty)
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

            int i;

            if (int.TryParse(cd.Value, out i))
                _lastValue = i;

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


        public static CallbackData GetCallback(string method, int i) => new CallbackData(method, i.ToString());
    }

    public class Int32Navigation<TForm> : IExternalAction
        where TForm : FormBase
    {
        public string Method { get; set; }

        public Action<TForm, int> SetProperty { get; set; }

        int? _lastValue { get; set; }

        public Int32Navigation(string method, Action<TForm, int> setProperty)
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

            int g;

            if (int.TryParse(cd.Value, out g))
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
    }
}
