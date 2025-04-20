using System;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotBase.DependencyInjection;
using TelegramBotBase.Exceptions;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Factories;

public class ServiceProviderStartFormFactory : IStartFormFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Type _startFormClass;

    public ServiceProviderStartFormFactory(Type startFormClass, IServiceProvider serviceProvider)
    {
        if (!typeof(FormBase).IsAssignableFrom(startFormClass))
        {
            throw new ArgumentException($"{nameof(startFormClass)} argument must be a {nameof(FormBase)} type");
        }
        
        _startFormClass = startFormClass;
        _serviceProvider = serviceProvider;
    }

    public FormBase CreateForm()
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

public class ServiceProviderStartFormFactory<T> : ServiceProviderStartFormFactory
    where T : FormBase
{
    public ServiceProviderStartFormFactory(IServiceProvider serviceProvider) : base(typeof(T), serviceProvider)
    {
    }
}