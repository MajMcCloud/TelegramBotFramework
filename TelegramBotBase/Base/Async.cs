using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotBase.Base
{
    public static class Async
    {
        public delegate Task AsyncEventHandler<TEventArgs>(object sender, TEventArgs e) where TEventArgs : EventArgs;

        public static IEnumerable<AsyncEventHandler<TEventArgs>> GetHandlers<TEventArgs>(
        this AsyncEventHandler<TEventArgs> handler)
        where TEventArgs : EventArgs
        => handler.GetInvocationList().Cast<AsyncEventHandler<TEventArgs>>();

        public static Task InvokeAllAsync<TEventArgs>(this AsyncEventHandler<TEventArgs> handler, object sender, TEventArgs e)
         where TEventArgs : EventArgs
         => Task.WhenAll(
             handler.GetHandlers()
             .Select(handleAsync => handleAsync(sender, e)));

    }
}
