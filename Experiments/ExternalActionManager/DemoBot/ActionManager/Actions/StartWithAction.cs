using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace DemoBot.ActionManager.Actions
{

    public class StartWithAction : IExternalAction
    {
        public string SearchForString { get; set; }

        public Action<FormBase, string> SetProperty { get; set; }

        Func<String, UpdateResult, MessageResult, Task> Action;


        public StartWithAction(string searchFor, Func<String, UpdateResult, MessageResult, Task> action)
        {
            SearchForString = searchFor;
            Action = action;
        }

        public bool DoesFit(string raw_data) => raw_data.StartsWith(SearchForString);
      

        public async Task DoAction(String raw_data, UpdateResult ur, MessageResult mr) => await Action(raw_data, ur, mr);

    }

    public static class StartWithAction_Extensions
    {
        public static void AddStartsWithAction(this ExternalActionManager manager, string value, Func<String, UpdateResult, MessageResult, Task> action)
        {
            manager.Add(new StartWithAction(value, action));
        }
    }

}
