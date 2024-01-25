using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Experiments.ActionManager.Actions
{

    public class EndsWithAction : IExternalAction
    {
        public string SearchForString { get; set; }

        public Action<FormBase, string> SetProperty { get; set; }

        Func<String, UpdateResult, MessageResult, Task> Action;


        public EndsWithAction(string searchFor, Func<String, UpdateResult, MessageResult, Task> action)
        {
            SearchForString = searchFor;
            Action = action;
        }

        public bool DoesFit(string raw_data) => raw_data.EndsWith(SearchForString);


        public async Task DoAction(UpdateResult ur, MessageResult mr) => await Action(mr.RawData, ur, mr);

    }

    public static class EndsWithAction_Extensions
    {
        public static void AddEndsWithAction(this ExternalActionManager manager, string value, Func<String, UpdateResult, MessageResult, Task> action)
        {
            manager.Add(new EndsWithAction(value, action));
        }
    }

}
