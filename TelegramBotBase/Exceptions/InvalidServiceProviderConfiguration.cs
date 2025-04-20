using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotBase.Exceptions
{
    public sealed class InvalidServiceProviderConfiguration : Exception
    {
        public InvalidServiceProviderConfiguration(string message, Exception innerException) : base(message, innerException) { }

    }
}
