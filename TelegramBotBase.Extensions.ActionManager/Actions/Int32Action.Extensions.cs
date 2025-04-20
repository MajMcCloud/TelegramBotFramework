using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces.ExternalActions;

namespace TelegramBotBase.Extensions.ActionManager.Actions
{
    public static class Int32Action_Extensions
    {

        public static void AddInt32Action(this IExternalActionManager manager, string method, Func<int, CallbackData, UpdateResult, MessageResult, Task> action)
        {
            manager.Add(new Int32Action(method, action));
        }
    }
}
