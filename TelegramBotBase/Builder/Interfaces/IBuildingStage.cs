namespace TelegramBotBase.Builder.Interfaces;

public interface IBuildingStage
{
    /// <summary>
    /// Creates a full BotBase instance with all parameters previously set.
    /// </summary>
    /// <returns></returns>
    BotBase Build();
}