using TelegramBotBase.Localizations;

namespace TelegramBotBase.Builder.Interfaces;

/// <summary>
/// Represents the language selection stage in the localization process.
/// </summary>
public interface ILanguageSelectionStage
{
    /// <summary>
    ///     Selects the default language for control usage. (English)
    /// </summary>
    /// <returns>The next stage in the building process.</returns>
    IBuildingStage DefaultLanguage();

    /// <summary>
    ///     Selects english as the default language for control labels.
    /// </summary>
    /// <returns>The next stage in the building process.</returns>
    IBuildingStage UseEnglish();

    /// <summary>
    ///     Selects german as the default language for control labels.
    /// </summary>
    /// <returns>The next stage in the building process.</returns>
    IBuildingStage UseGerman();

    /// <summary>
    ///     Selects persian as the default language for control labels.
    /// </summary>
    /// <returns>The next stage in the building process.</returns>
    IBuildingStage UsePersian();

    /// <summary>
    ///     Selects russian as the default language for control labels.
    /// </summary>
    /// <returns>The next stage in the building process.</returns>
    IBuildingStage UseRussian();

    /// <summary>
    ///     Selects a custom language as the default language for control labels.
    /// </summary>
    /// <returns>The next stage in the building process.</returns>
    IBuildingStage Custom(Localization language);
}