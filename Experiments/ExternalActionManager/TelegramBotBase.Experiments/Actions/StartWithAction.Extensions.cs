using TelegramBotBase.Base;

namespace TelegramBotBase.Experiments.ActionManager.Actions
{
    public static class StartWithAction_Extensions
    {
        public static void AddStartsWithAction(this IExternalActionManager manager, string value, Func<String, UpdateResult, MessageResult, Task> action)
        {
            manager.Add(new StartWithAction(value, action));
        }
    }

}
