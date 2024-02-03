using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Experiments.ActionManager.Actions
{
    public class StringAction : IExternalAction
    {
        public string Method { get; set; }

        CallbackData? _lastData { get; set; }

        String? _lastValue { get; set; }

        Func<string, CallbackData, UpdateResult, MessageResult, Task> Action;

        public StringAction(string method, Func<string, CallbackData, UpdateResult, MessageResult, Task> action)
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

            _lastValue = cd.Value;

            _lastData = cd;

            return true;
        }


        public async Task DoAction(UpdateResult ur, MessageResult mr) => await Action(_lastValue, _lastData, ur, mr);

        public static CallbackData GetCallback(string method, string str) => new CallbackData(method, str);

    }

    public class StringAction<TForm> : IExternalAction
        where TForm : FormBase
    {
        public string Method { get; set; }

        CallbackData? _lastData { get; set; }

        String? _lastValue { get; set; }

        Func<String, CallbackData, UpdateResult, MessageResult, Task> Action;

        public StringAction(string method, Func<String, CallbackData, UpdateResult, MessageResult, Task> action)
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

            _lastValue = cd.Value;

            _lastData = cd;

            return true;
        }


        public async Task DoAction(UpdateResult ur, MessageResult mr) => await Action(_lastValue, _lastData, ur, mr);

    }
}
