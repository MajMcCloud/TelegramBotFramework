using System;
using System.Collections.Generic;
using System.Text;
using TelegramBotBase.Form;

namespace TelegramBotBase.Factories
{
    public class LambdaStartFormFactory : Interfaces.IStartFormFactory
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
}
