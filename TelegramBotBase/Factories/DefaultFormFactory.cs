using System;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Factories;

public class DefaultFormFactory : IFormFactory
{
    private readonly Type _startFormClass;

    public DefaultFormFactory(Type startFormClass)
    {
        if (!typeof(FormBase).IsAssignableFrom(startFormClass))
        {
            throw new ArgumentException($"{nameof(startFormClass)} argument must be a {nameof(FormBase)} type");
        }

        _startFormClass = startFormClass;
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

        // No parameterless constructor
        if (!(formType.GetConstructor(new Type[] { })?.Invoke(new object[] { }) is FormBase form))
        {
            throw new Exception($"{formType} must have a parameterless constructor.");
        }

        return form;
    }

    public FormBase CreateForm<T>() where T : FormBase
    {
        return CreateForm(typeof(T));
    }
}