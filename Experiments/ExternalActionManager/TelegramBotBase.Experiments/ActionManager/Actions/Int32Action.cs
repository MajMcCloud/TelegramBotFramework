using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Experiments.ActionManager.Actions
{
    public class Int32Action : IExternalAction
    {
        public string Method { get; set; }

        int? _lastValue { get; set; }

        Func<int, UpdateResult, MessageResult, Task> Action;

        public Int32Action(string method, Func<int, UpdateResult, MessageResult, Task> action)
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

            return true;
        }


        public async Task DoAction(UpdateResult ur, MessageResult mr) => await Action(_lastValue.Value, ur, mr);

        public static CallbackData GetCallback(string method, long l) => new CallbackData(method, l.ToString());

    }

    public class Int32Action<TForm> : IExternalAction
        where TForm : FormBase
    {
        public string Method { get; set; }

        int? _lastValue { get; set; }

        Func<int, UpdateResult, MessageResult, Task> Action;

        public Int32Action(string method, Func<int, UpdateResult, MessageResult, Task> action)
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

            return true;
        }


        public async Task DoAction(UpdateResult ur, MessageResult mr) => await Action(_lastValue.Value, ur, mr);
    }
}
