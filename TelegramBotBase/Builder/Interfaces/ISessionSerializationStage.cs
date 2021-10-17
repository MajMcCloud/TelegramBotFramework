using System;
using System.Collections.Generic;
using System.Text;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Builder.Interfaces
{
    public interface ISessionSerializationStage
    {
        /// <summary>
        /// Do not uses serialization.
        /// </summary>
        /// <returns></returns>
        IBuildingStage NoSerialization();

        /// <summary>
        /// Sets the state machine for serialization.
        /// </summary>
        /// <param name="machine"></param>
        /// <returns></returns>
        IBuildingStage UseSerialization(IStateMachine machine);

    }
}
