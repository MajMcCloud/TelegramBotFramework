using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces.ExternalActions;

namespace TelegramBotBase.Extensions.ActionManager.Actions
{
    public class Int32Action : IExternalAction
    {
        public string Method { get; set; }

        CallbackData _lastData { get; set; }

        int? _lastValue { get; set; }

        Func<int, CallbackData, UpdateResult, MessageResult, Task> Action;

        public Int32Action(string method, Func<int, CallbackData, UpdateResult, MessageResult, Task> action)
        {
            Method = method;
            Action = action;
        }


        public bool DoesFit(string raw_data)
        {
            var cd = CallbackData.Deserialize(raw_data);

            if (cd == null)
                return false;

            if (cd.Method != Method)
                return false;

            int i;

            if (int.TryParse(cd.Value, out i))
                _lastValue = i;

            _lastData = cd;

            return true;
        }


        public async Task DoAction(UpdateResult ur, MessageResult mr) => await Action(_lastValue.Value, _lastData, ur, mr);

        public static CallbackData GetCallback(string method, int l) => new CallbackData(method, l.ToString());

    }

    public class Int32Action<TForm> : IExternalAction
        where TForm : FormBase
    {
        public string Method { get; set; }

        CallbackData _lastData { get; set; }

        int? _lastValue { get; set; }

        Func<int, CallbackData, UpdateResult, MessageResult, Task> Action;

        public Int32Action(string method, Func<int, CallbackData, UpdateResult, MessageResult, Task> action)
        {
            Method = method;
            Action = action;
        }

        public bool DoesFit(string raw_data)
        {
            var cd = CallbackData.Deserialize(raw_data);

            if (cd == null)
                return false;

            if (cd.Method != Method)
                return false;

            int i;

            if (int.TryParse(cd.Value, out i))
                _lastValue = i;

            _lastData = cd;

            return true;
        }


        public async Task DoAction(UpdateResult ur, MessageResult mr) => await Action(_lastValue.Value, _lastData, ur, mr);
    }
}
