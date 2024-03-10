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
    IThreadingStage DefaultLanguage();

    /// <summary>
    ///     Selects english as the default language for control labels.
    /// </summary>
    /// <returns>The next stage in the building process.</returns>
    IThreadingStage UseEnglish();

    /// <summary>
    ///     Selects german as the default language for control labels.
    /// </summary>
    /// <returns>The next stage in the building process.</returns>
    IThreadingStage UseGerman();

    /// <summary>
    ///     Selects persian as the default language for control labels.
    /// </summary>
    /// <returns>The next stage in the building process.</returns>
    IThreadingStage UsePersian();

    /// <summary>
    ///     Selects russian as the default language for control labels.
    /// </summary>
    /// <returns>The next stage in the building process.</returns>
    IThreadingStage UseRussian();

    /// <summary>
    ///     Selects a custom language as the default language for control labels.
    /// </summary>
    /// <returns>The next stage in the building process.</returns>
    IThreadingStage Custom(Localization language);
}