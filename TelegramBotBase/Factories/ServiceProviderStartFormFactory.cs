using System;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Factories
{
    public class ServiceProviderStartFormFactory : IStartFormFactory
    {
        private readonly Type _startFormClass;
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderStartFormFactory(Type startFormClass, IServiceProvider serviceProvider)
        {
            if (!typeof(FormBase).IsAssignableFrom(startFormClass))
                throw new ArgumentException("startFormClass argument must be a FormBase type");

            _startFormClass = startFormClass;
            _serviceProvider = serviceProvider;
        }

        public FormBase CreateForm()
        {
            return (FormBase)ActivatorUtilities.CreateInstance(_serviceProvider, _startFormClass);
        }
    }

    public class ServiceProviderStartFormFactory<T> : ServiceProviderStartFormFactory
        where T : FormBase
    {
        public ServiceProviderStartFormFactory(IServiceProvider serviceProvider) : base(typeof(T), serviceProvider)
        {
        }
    }
}
