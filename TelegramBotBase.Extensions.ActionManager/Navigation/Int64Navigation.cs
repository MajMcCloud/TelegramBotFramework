using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces.ExternalActions;

namespace TelegramBotBase.Extensions.ActionManager.Navigation
{
    public class Int64Navigation : IExternalAction
    {
        public Type FormType { get; }

        public string Method { get; set; }

        public Action<FormBase, long> SetProperty { get; set; }

        long? _lastValue { get; set; }

        public Int64Navigation(Type formType, string method, Action<FormBase, long> setProperty)
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

            long l;

            if (long.TryParse(cd.Value, out l))
                _lastValue = l;

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


        public static CallbackData GetCallback(string method, long l) => new CallbackData(method, l.ToString());
    }

    public class Int64Navigation<TForm> : IExternalAction
        where TForm : FormBase
    {
        public string Method { get; set; }

        public Action<TForm, long> SetProperty { get; set; }

        long? _lastValue { get; set; }

        public Int64Navigation(string method, Action<TForm, long> setProperty)
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

            long l;

            if (long.TryParse(cd.Value, out l))
                _lastValue = l;

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
