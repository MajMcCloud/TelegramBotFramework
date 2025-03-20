using TelegramBotBase.Base;

namespace TelegramBotBase.Experiments.ActionManager.Actions
{
    public static class EndsWithAction_Extensions
    {
        public static void AddEndsWithAction(this IExternalActionManager manager, string value, Func<String, UpdateResult, MessageResult, Task> action)
        {
            manager.Add(new EndsWithAction(value, action));
        }
    }

}
