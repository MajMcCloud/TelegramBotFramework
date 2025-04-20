using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces.ExternalActions;

namespace TelegramBotBase.Extensions.ActionManager.Actions
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

}
