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


        /// <summary>
        /// Using the complex version of .Net JSON, which can serialize all objects.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        IBuildingStage UseJSON(String path);


        /// <summary>
        /// Use the easy version of .Net JSON, which can serialize basic types, but not generics and others.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        IBuildingStage UseSimpleJSON(String path);


        /// <summary>
        /// Uses the XML serializer for session serialization.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        IBuildingStage UseXML(String path);

    }
}
