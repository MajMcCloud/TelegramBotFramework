using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Experiments.ActionManager.Actions
{
    public static class StringAction_Extensions
    {

        public static void AddStringAction(this ExternalActionManager manager, string method, Func<String, CallbackData, UpdateResult, MessageResult, Task> action)
        {
            manager.Add(new StringAction(method, action));
        }
    }
}
