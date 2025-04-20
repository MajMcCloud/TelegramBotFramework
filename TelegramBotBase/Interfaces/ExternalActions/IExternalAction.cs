using System.Threading.Tasks;
using TelegramBotBase.Base;

namespace TelegramBotBase.Interfaces.ExternalActions
{

    public interface IExternalAction
    {
        bool DoesFit(string raw_data);

        Task DoAction(UpdateResult ur, MessageResult mr);
    }

}
