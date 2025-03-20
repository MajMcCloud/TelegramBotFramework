using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Experiments.ActionManager.Actions
{
    public static class Int64Action_Extensions
    {

        public static void AddInt64Action(this IExternalActionManager manager, string method, Func<long, CallbackData, UpdateResult, MessageResult, Task> action)
        {
            manager.Add(new Int64Action(method, action));
        }
    }
}
