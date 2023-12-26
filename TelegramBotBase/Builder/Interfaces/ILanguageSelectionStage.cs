using TelegramBotBase.Localizations;

namespace TelegramBotBase.Builder.Interfaces;

public interface ILanguageSelectionStage
{
    /// <summary>
    ///     Selects the default language for control usage. (English)
    /// </summary>
    /// <returns></returns>
    IThreadingStage DefaultLanguage();

    /// <summary>
    ///     Selects english as the default language for control labels.
    /// </summary>
    /// <returns></returns>
    IThreadingStage UseEnglish();

    /// <summary>
    ///     Selects german as the default language for control labels.
    /// </summary>
    /// <returns></returns>
    IThreadingStage UseGerman();

    /// <summary>
    ///     Selects persian as the default language for control labels.
    /// </summary>
    /// <returns></returns>
    IThreadingStage UsePersian();

    /// <summary>
    ///     Selects a custom language as the default language for control labels.
    /// </summary>
    /// <returns></returns>
    IThreadingStage Custom(Localization language);
}