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
        return _startFormClass.GetConstructor(new Type[] { })?.Invoke(new object[] { }) as FormBase;
    }
}