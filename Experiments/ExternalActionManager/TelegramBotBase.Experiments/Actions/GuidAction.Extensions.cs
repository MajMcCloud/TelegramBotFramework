using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Experiments.ActionManager.Actions
{
    public static class GuidAction_Extensions
    {

        public static void AddGuidAction(this IExternalActionManager manager, string method, Func<Guid, CallbackData, UpdateResult, MessageResult, Task> action)
        {
            manager.Add(new GuidAction(method, action));
        }
    }
}
