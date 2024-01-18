using TelegramBotBase.Base;
using TelegramBotBase.Interfaces;
using TelegramBotBase.Sessions;

namespace DemoBot.ActionManager
{

    public interface IExternalAction
    {
        bool DoesFit(string raw_action);

        Task DoAction(UpdateResult ur, MessageResult mr, DeviceSession session);
    }

}
