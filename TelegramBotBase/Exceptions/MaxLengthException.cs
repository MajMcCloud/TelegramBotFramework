using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotBase.Exceptions
{
    public class MaxLengthException : Exception
    {
        public MaxLengthException(int length) : base($"Your messages with a length of {length} is too long for telegram. Actually is {Constants.Telegram.MaxMessageLength} characters allowed. Please split it.")
        {
            
        }

    }
}
