using System;

namespace TelegramBotBase.Base
{
    public class ErrorResult : EventArgs
    {
        public ErrorResult(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; }
    }
}
