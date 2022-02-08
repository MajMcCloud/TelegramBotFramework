using System;
using System.Collections.Generic;
using System.Text;
using TelegramBotBase.Localizations;

namespace TelegramBotBase.Builder.Interfaces
{
    public interface ILanguageSelectionStage
    {

        /// <summary>
        /// Selects the default language for control usage. (English)
        /// </summary>
        /// <returns></returns>
        IBuildingStage DefaultLanguage();

        /// <summary>
        /// Selects english as the default language for control labels.
        /// </summary>
        /// <returns></returns>
        IBuildingStage UseEnglish();

        /// <summary>
        /// Selects german as the default language for control labels.
        /// </summary>
        /// <returns></returns>
        IBuildingStage UseGerman();

        /// <summary>
        /// Selects a custom language as the default language for control labels.
        /// </summary>
        /// <returns></returns>
        IBuildingStage Custom(Localization language);

    }
}
