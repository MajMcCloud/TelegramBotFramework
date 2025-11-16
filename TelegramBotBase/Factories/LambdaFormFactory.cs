using System;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Factories;

public class LambdaFormFactory : IFormFactory
{
    public delegate FormBase CreateFormDelegate();

    private readonly CreateFormDelegate _lambda;
    
    private readonly DefaultFormFactory _defaultFormFactory;

    public LambdaFormFactory(CreateFormDelegate lambda)
    {
        _lambda = lambda;
        _defaultFormFactory = new DefaultFormFactory(lambda.GetType());
    }

    public FormBase CreateStartForm()
    {
        return _lambda();
    }

    public FormBase CreateForm(Type formType)
    {
        return _defaultFormFactory.CreateForm(formType);
    }

    public FormBase CreateForm<T>() where T : FormBase
    {
        return CreateForm(typeof(T));
    }
}