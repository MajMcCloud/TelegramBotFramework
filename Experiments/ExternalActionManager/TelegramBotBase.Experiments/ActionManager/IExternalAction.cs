using TelegramBotBase.Base;

namespace TelegramBotBase.Experiments.ActionManager
{

    public interface IExternalAction
    {
        bool DoesFit(string raw_data);

        Task DoAction(UpdateResult ur, MessageResult mr);
    }

}
