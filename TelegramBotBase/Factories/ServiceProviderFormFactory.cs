using System;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotBase.DependencyInjection;
using TelegramBotBase.Exceptions;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Factories;

public class ServiceProviderFormFactory : IFormFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Type _startFormClass;

    public ServiceProviderFormFactory(Type startFormClass, IServiceProvider serviceProvider)
    {
        if (!typeof(FormBase).IsAssignableFrom(startFormClass))
        {
            throw new ArgumentException($"{nameof(startFormClass)} argument must be a {nameof(FormBase)} type");
        }
        
        _startFormClass = startFormClass;
        _serviceProvider = serviceProvider;
    }

    public FormBase CreateStartForm()
    {
        FormBase fb = null;

        try
        {
            fb = (FormBase)ActivatorUtilities.CreateInstance(_serviceProvider, _startFormClass);
        }
        catch(InvalidOperationException ex)
        {
            throw new InvalidServiceProviderConfiguration(ex.Message, ex);
        }

        //Sets an internal field for future ServiceProvider navigation
        fb.SetServiceProvider(_serviceProvider);

        return fb;
    }
}

public class ServiceProviderFormFactory<T> : ServiceProviderFormFactory
    where T : FormBase
{
    public ServiceProviderFormFactory(IServiceProvider serviceProvider) : base(typeof(T), serviceProvider)
    {
    }
}