using System;
using Telegram.Bot.Exceptions;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.Base
{
    public class SimpleStartFormFactory : IStartFormFactory
    {
        private readonly Type _startFormClass;

        public SimpleStartFormFactory(Type startFormClass)
        {
            if (!typeof(FormBase).IsAssignableFrom(startFormClass))
                throw new ArgumentException("startFormClass argument must be a FormBase type");
            
            _startFormClass = startFormClass;
        }

        public FormBase CreateForm()
        {
            return _startFormClass.GetConstructor(new Type[] { })?.Invoke(new object[] { }) as FormBase;
        }
    }
}