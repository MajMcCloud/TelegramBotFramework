using System;
using System.Collections.Generic;
using System.Text;
using TelegramBotBase.Form;

namespace TelegramBotBase.Interfaces
{
    public interface IStartFormFactory
    {
        FormBase CreateForm();
    }
}
