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
        return CreateForm(_startFormClass);
    }
    
    public FormBase CreateForm(Type formType)
    {
        if (!typeof(FormBase).IsAssignableFrom(formType))
        {
            throw new ArgumentException($"{nameof(formType)} argument must be a {nameof(FormBase)} type");
        }

        FormBase fb = null;

        try
        {
            fb = (FormBase)ActivatorUtilities.CreateInstance(_serviceProvider, formType);
        }
        catch(InvalidOperationException ex)
        {
            throw new InvalidServiceProviderConfiguration(ex.Message, ex);
        }

        //Sets an internal field for future ServiceProvider navigation
        fb.SetServiceProvider(_serviceProvider);

        return fb;
    }

    public FormBase CreateForm<T>() where T : FormBase, new()
    {
        return CreateForm(typeof(T));
    }
}

public class ServiceProviderFormFactory<T> : ServiceProviderFormFactory
    where T : FormBase
{
    public ServiceProviderFormFactory(IServiceProvider serviceProvider) : base(typeof(T), serviceProvider)
    {
    }
}