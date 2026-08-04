using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.DependencyInjection;

public sealed class FormDiEscort : IAsyncDisposable
{
    public IServiceScope ServiceScope;
    public IFormFactory FormFactory;
    
    public async ValueTask DisposeAsync()
    {
        FormFactory = null;

        switch (ServiceScope)
        {
            case null:
            {
                return;
            }
            case IAsyncDisposable ad:
            {
                await ad.DisposeAsync();
                break;
            }
            default:
            {
                ServiceScope.Dispose();
                break;
            }
        }

        ServiceScope = null;
    }
}