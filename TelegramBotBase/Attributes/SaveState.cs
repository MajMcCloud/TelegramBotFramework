using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotBase.Attributes
{
    /// <summary>
    /// Declares that the field or property should be save and recovered an restart.
    /// </summary>
    public class SaveState : Attribute
    {
        public String Key { get; set; }

    }
}
