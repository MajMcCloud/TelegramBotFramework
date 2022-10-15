using System;
using System.Threading.Tasks;
using TelegramBotBase.Form;
using TelegramBotBase.Form.Navigation;

namespace TelegramBotBase.Example.Tests.Navigation;

internal class CustomController : NavigationController
{
    public CustomController(FormBase form) : base(form)
    {
    }


    public override Task PushAsync(FormBase form, params object[] args)
    {
        Console.WriteLine($"Pushes form (Count on stack {Index + 1})");
        //Device.Send($"Pushes form (Count on stack {this.Index + 1})");

        return base.PushAsync(form, args);
    }


    public override Task PopAsync()
    {
        Console.WriteLine($"Pops one form (Count on stack {Index + 1})");
        //Device.Send($"Pops one form (Count on stack {this.Index + 1})");

        return base.PopAsync();
    }

    public override Task PopToRootAsync()
    {
        Console.WriteLine($"Moved back to root (Count on stack {Index + 1})");
        //Device.Send($"Moved back to root (Count on stack {this.Index + 1})");

        return base.PopToRootAsync();
    }
}