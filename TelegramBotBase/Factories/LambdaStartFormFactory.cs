using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Factories;

public class LambdaStartFormFactory : IStartFormFactory
{
    public delegate FormBase CreateFormDelegate();

    private readonly CreateFormDelegate _lambda;

    public LambdaStartFormFactory(CreateFormDelegate lambda)
    {
        _lambda = lambda;
    }

    public FormBase CreateForm()
    {
        return _lambda();
    }
}