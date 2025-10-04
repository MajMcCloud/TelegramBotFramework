using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Factories;

public class LambdaFormFactory : IFormFactory
{
    public delegate FormBase CreateFormDelegate();

    private readonly CreateFormDelegate _lambda;

    public LambdaFormFactory(CreateFormDelegate lambda)
    {
        _lambda = lambda;
    }

    public FormBase CreateStartForm()
    {
        return _lambda();
    }
}