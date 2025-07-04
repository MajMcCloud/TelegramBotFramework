﻿using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces.ExternalActions;

namespace TelegramBotBase.Extensions.ActionManager.Actions
{
    public class Int64Action : IExternalAction
    {
        public string Method { get; set; }

        CallbackData _lastData { get; set; }

        long? _lastValue { get; set; }

        Func<long, CallbackData, UpdateResult, MessageResult, Task> Action;

        public Int64Action(string method, Func<long, CallbackData, UpdateResult, MessageResult, Task> action)
        {
            Method = method;
            Action = action;
        }

        public bool DoesFit(string raw_data)
        {
            if (!CallbackData.TryDeserialize(raw_data, out CallbackData cd))
                return false;

            if (cd == null)
                return false;

            if (cd.Method != Method)
                return false;

            long l;

            if (long.TryParse(cd.Value, out l))
                _lastValue = l;

            _lastData = cd;

            return true;
        }


        public async Task DoAction(UpdateResult ur, MessageResult mr) => await Action(_lastValue.Value, _lastData, ur, mr);

        public static CallbackData GetCallback(string method, long l) => new CallbackData(method, l.ToString());

    }

    public class Int64Action<TForm> : IExternalAction
        where TForm : FormBase
    {
        public string Method { get; set; }

        CallbackData _lastData { get; set; }

        long? _lastValue { get; set; }

        Func<long, CallbackData, UpdateResult, MessageResult, Task> Action;

        public Int64Action(string method, Func<long, CallbackData, UpdateResult, MessageResult, Task> action)
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

            long g;

            if (long.TryParse(cd.Value, out g))
                _lastValue = g;

            _lastData = cd;

            return true;
        }


        public async Task DoAction(UpdateResult ur, MessageResult mr) => await Action(_lastValue.Value, _lastData, ur, mr);
    }
}
