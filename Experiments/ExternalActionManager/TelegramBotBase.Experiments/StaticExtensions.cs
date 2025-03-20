using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Builder.Interfaces;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Experiments.ActionManager
{
    public static class StaticExtensions
    {
        public static IStartFormSelectionStage ActionMessageLoop(this IMessageLoopSelectionStage loopSelection, IExternalActionManager managerInstance)
        {
            var cfb = new CustomFormBaseMessageLoop();

            cfb.ExternalActionManager = managerInstance;
            
            return loopSelection.CustomMessageLoop(cfb);
        }




    }
}
