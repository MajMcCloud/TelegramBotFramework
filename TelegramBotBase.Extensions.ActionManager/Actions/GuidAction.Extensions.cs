using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces.ExternalActions;

namespace TelegramBotBase.Extensions.ActionManager.Actions
{
    public static class GuidAction_Extensions
    {

        public static void AddGuidAction(this IExternalActionManager manager, string method, Func<Guid, CallbackData, UpdateResult, MessageResult, Task> action)
        {
            manager.Add(new GuidAction(method, action));
        }
    }
}
