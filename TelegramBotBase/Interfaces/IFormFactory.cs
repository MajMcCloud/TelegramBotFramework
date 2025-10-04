using TelegramBotBase.Form;

namespace TelegramBotBase.Interfaces;

public interface IFormFactory
{
    FormBase CreateStartForm();
}