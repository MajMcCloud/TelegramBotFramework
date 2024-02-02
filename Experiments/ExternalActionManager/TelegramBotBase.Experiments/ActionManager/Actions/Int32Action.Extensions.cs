using TelegramBotBase.Base;

namespace TelegramBotBase.Experiments.ActionManager.Actions
{
    public static class Int32Action_Extensions
    {

        public static void AddInt32Action(this ExternalActionManager manager, string method, Func<int, UpdateResult, MessageResult, Task> action)
        {
            manager.Add(new Int32Action(method, action));
        }
    }
}
