using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Example.Tests.Groups;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests;

public class Start : SplitterForm
{
    public override async Task<bool> Open(MessageResult e)
    {
        var st = new Menu();
        await NavigateTo(st);

        return true;
    }


    public override async Task<bool> OpenGroup(MessageResult e)
    {
        var st = new LinkReplaceTest();
        await NavigateTo(st);

        return true;
    }

    public override Task<bool> OpenChannel(MessageResult e)
    {
        return base.OpenChannel(e);
    }

    public override Task<bool> OpenSupergroup(MessageResult e)
    {
        return base.OpenSupergroup(e);
    }
}