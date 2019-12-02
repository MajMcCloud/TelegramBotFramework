using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotBase.Exceptions
{
    public class MaximumRowsReachedException : Exception
    {
        public int Value { get; set; }

        public int Maximum { get; set; }


        public override string Message
        {
            get
            {
                return $"You have exceeded the maximum of rows by {Value.ToString()} / {Maximum.ToString()}";
            }
        }
    }
}
