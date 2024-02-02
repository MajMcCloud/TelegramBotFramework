using TelegramBotBase.Base;

namespace TelegramBotBase.Experiments.ActionManager.Actions
{
    public static class Int64Action_Extensions
    {

        public static void AddInt64Action(this ExternalActionManager manager, string method, Func<long, UpdateResult, MessageResult, Task> action)
        {
            manager.Add(new Int64Action(method, action));
        }
    }
}
