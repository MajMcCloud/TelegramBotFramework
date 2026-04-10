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

        if (ServiceScope == null) return;

        if (ServiceScope is IAsyncDisposable ad)
        {
            await ad.DisposeAsync();
        }
        else
        {
            ServiceScope.Dispose();
        }
        
        ServiceScope = null;
    }
}