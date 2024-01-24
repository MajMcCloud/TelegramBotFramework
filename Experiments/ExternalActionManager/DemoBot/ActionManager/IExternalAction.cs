using TelegramBotBase.Base;

namespace DemoBot.ActionManager
{

    public interface IExternalAction
    {
        bool DoesFit(string raw_data);

        Task DoAction(UpdateResult ur, MessageResult mr);
    }

}
