using Microsoft.Extensions.DependencyInjection;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.DependencyInjection;

public sealed class FormDiEscort
{
    public IServiceScope ServiceScope;
    public IFormFactory FormFactory;
}