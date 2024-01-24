using TelegramBotBase.Base;

namespace DemoBot.ActionManager
{
    public partial class ExternalActionManager
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

                await action.DoAction(mr.RawData, ur, mr);

                return true;
            }


            return false;
        }

        /// <summary>
        /// Creates an instance of the ExternalActionManager for configuration.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static ExternalActionManager Configure(Action<ExternalActionManager> action)
        {
            var eam = new ExternalActionManager();

            action(eam);

            return eam;
        }
    }
}
