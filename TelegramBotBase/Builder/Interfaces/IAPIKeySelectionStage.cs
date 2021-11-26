using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotBase.Builder.Interfaces
{
    public interface IAPIKeySelectionStage
    {
        /// <summary>
        /// Sets the API Key which will be used by the telegram bot client.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        IMessageLoopSelectionStage WithAPIKey(String apiKey);

    }
}
