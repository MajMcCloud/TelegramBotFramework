using System;
using TelegramBotBase.Form;

namespace TelegramBotBase.Interfaces;

public interface IFormFactory
{
    FormBase CreateStartForm();
    FormBase CreateForm(Type formType);
    FormBase CreateForm<T>() where T : FormBase, new();
}