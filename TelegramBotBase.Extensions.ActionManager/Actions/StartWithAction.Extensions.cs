using TelegramBotBase.Base;
using TelegramBotBase.Interfaces.ExternalActions;

namespace TelegramBotBase.Extensions.ActionManager.Actions
{
    public static class StartWithAction_Extensions
    {
        public static void AddStartsWithAction(this IExternalActionManager manager, string value, Func<String, UpdateResult, MessageResult, Task> action)
        {
            manager.Add(new StartWithAction(value, action));
        }
    }

}
