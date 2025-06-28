using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces.ExternalActions;

namespace TelegramBotBase.Extensions.ActionManager.Actions
{
    public static class StringAction_Extensions
    {

        public static void AddStringAction(this IExternalActionManager manager, string method, Func<String, CallbackData, UpdateResult, MessageResult, Task> action)
        {
            manager.Add(new StringAction(method, action));
        }
    }
}
