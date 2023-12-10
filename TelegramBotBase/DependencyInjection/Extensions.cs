using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Form;

namespace TelegramBotBase.DependencyInjection
{
    public static class Extensions
    {
        internal static FieldInfo _ServiceProviderField = typeof(FormBase).GetField("_serviceProvider", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        /// <summary>
        /// Use Dependency Injection to create new form and inject parameters. (Main variant)
        /// </summary>
        /// <typeparam name="NewForm"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task<NewForm> NavigateTo<NewForm>(this FormBase current_form, params object[] args)
            where NewForm : FormBase
        {
            var _serviceProvider = current_form.GetServiceProvider();

            var instance = ActivatorUtilities.CreateInstance(_serviceProvider, typeof(NewForm)) as NewForm;

            if (instance == null)
                return null; //throw new Exception("Could not instantiate new form via DI.");

            instance.SetServiceProvider(_serviceProvider);

            await current_form.NavigateTo(instance, args);

            return instance;
        }

        /// <summary>
        /// Use Dependency Injection to create new form and inject parameters. (Alternative variant)
        /// </summary>
        /// <param name="current_form"></param>
        /// <param name="formBaseType"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<FormBase> NavigateTo(this FormBase current_form, Type formBaseType, params object[] args)
        {
            if (!typeof(FormBase).IsAssignableFrom(formBaseType))
                throw new ArgumentException($"{nameof(formBaseType)} argument must be a {nameof(FormBase)} type");

            var _serviceProvider = current_form.GetServiceProvider();

            var instance = ActivatorUtilities.CreateInstance(_serviceProvider, formBaseType) as FormBase;

            if (instance == null)
                return null; //throw new Exception("Could not instantiate new form via DI.");

            instance.SetServiceProvider(_serviceProvider);

            await current_form.NavigateTo(instance, args);

            return instance;
        }

        /// <summary>
        /// Sets the internal service provider field.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="serviceProvider"></param>
        public static void SetServiceProvider(this FormBase form, IServiceProvider serviceProvider)
        {
            _ServiceProviderField?.SetValue(form, serviceProvider);
        }

        /// <summary>
        /// Gets the internal service provider field value.
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        public static IServiceProvider GetServiceProvider(this FormBase form)
        {
            var sp = _ServiceProviderField?.GetValue(form) as IServiceProvider;
            return sp;
        }
    }
}
