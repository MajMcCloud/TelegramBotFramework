using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;

namespace TelegramBotBase.Experiments.ActionManager
{
    public interface IExternalActionManager
    {
        Task<bool> ManageCall(UpdateResult ur, MessageResult mr);


        void Add(IExternalAction action);


    }
}
