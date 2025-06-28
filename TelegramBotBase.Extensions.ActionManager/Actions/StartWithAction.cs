using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces.ExternalActions;

namespace TelegramBotBase.Extensions.ActionManager.Actions
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
      

        public async Task DoAction(UpdateResult ur, MessageResult mr) => await Action(mr.RawData, ur, mr);

    }

}
