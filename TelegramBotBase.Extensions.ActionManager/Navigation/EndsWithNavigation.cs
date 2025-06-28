using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces.ExternalActions;

namespace TelegramBotBase.Extensions.ActionManager.Navigation
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


        public async Task DoAction(UpdateResult ur, MessageResult mr)
        {
            await mr.ConfirmAction();

            var new_form = FormType.GetConstructor(new Type[] { })?.Invoke(new object[] { }) as FormBase;

            if (mr.RawData != null)
            {
                SetProperty(new_form, mr.RawData);
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


        public async Task DoAction(UpdateResult ur, MessageResult mr)
        {
            await mr.ConfirmAction();

            var type = typeof(TForm);

            TForm new_form = type.GetConstructor(new Type[] { })?.Invoke(new object[] { }) as TForm;

            if (mr.RawData != null)
            {
                SetProperty(new_form, mr.RawData);
            }

            await ur.Device.ActiveForm.NavigateTo(new_form);
        }



    }

}
