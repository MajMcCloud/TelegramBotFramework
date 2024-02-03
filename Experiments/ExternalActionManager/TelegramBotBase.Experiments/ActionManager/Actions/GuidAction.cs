using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Experiments.ActionManager.Actions
{
    public class GuidAction : IExternalAction
    {
        public string Method { get; set; }

        CallbackData? _lastData { get; set; }

        Guid? _lastValue { get; set; }

        Func<Guid, CallbackData, UpdateResult, MessageResult, Task> Action;

        public GuidAction(string method, Func<Guid, CallbackData, UpdateResult, MessageResult, Task> action)
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

            Guid g;

            if (Guid.TryParse(cd.Value, out g))
                _lastValue = g;

            _lastData = cd;

            return true;
        }


        public async Task DoAction(UpdateResult ur, MessageResult mr) => await Action(_lastValue.Value, _lastData, ur, mr);

        public static CallbackData GetCallback(string method, Guid guid) => new CallbackData(method, guid.ToString());

    }

    public class GuidAction<TForm> : IExternalAction
        where TForm : FormBase
    {
        public string Method { get; set; }

        Guid? _lastValue { get; set; }

        Func<Guid, UpdateResult, MessageResult, Task> Action;

        public GuidAction(string method, Func<Guid, UpdateResult, MessageResult, Task> action)
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

            Guid g;

            if (Guid.TryParse(cd.Value, out g))
                _lastValue = g;

            return true;
        }


        public async Task DoAction(UpdateResult ur, MessageResult mr) => await Action(_lastValue.Value, ur, mr);
    }
}
