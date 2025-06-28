using TelegramBotBase.Base;
using TelegramBotBase.Interfaces.ExternalActions;

namespace TelegramBotBase.Extensions.ActionManager
{
    public partial class ExternalActionManager : IExternalActionManager
    {

        List<IExternalAction> actions = new List<IExternalAction>();

        public void Add(IExternalAction action)
        {
            actions.Add(action);
        }

        public async Task<bool> ManageCall(UpdateResult ur, MessageResult mr)
        {

            foreach (var action in actions)
            {
                if (!action.DoesFit(mr.RawData))
                    continue;

                await action.DoAction(ur, mr);

                return true;
            }


            return false;
        }

        /// <summary>
        /// Creates an instance of the ExternalActionManager for configuration.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IExternalActionManager Configure(Action<IExternalActionManager> action)
        {
            var eam = new ExternalActionManager();

            action(eam);

            return eam;
        }
    }
}
