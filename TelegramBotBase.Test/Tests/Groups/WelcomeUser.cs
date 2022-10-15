using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests.Groups;

public class WelcomeUser : GroupForm
{
    public WelcomeUser()
    {
        Opened += WelcomeUser_Opened;
    }


    private async Task WelcomeUser_Opened(object sender, EventArgs e)
    {
        var bf = new ButtonForm();

        bf.AddButtonRow(new ButtonBase("Open GroupChange Test", "groupchange"));
        bf.AddButtonRow(new ButtonBase("Open WelcomeUser Test", "welcomeuser"));
        bf.AddButtonRow(new ButtonBase("Open LinkReplace Test", "linkreplace"));

        await Device.Send("WelcomeUser started, click to switch", bf);
    }

    public override async Task Action(MessageResult message)
    {
        if (message.Handled)
        {
            return;
        }

        var bn = message.RawData;

        await message.ConfirmAction();
        message.Handled = true;

        switch (bn)
        {
            case "groupchange":

                var gc = new GroupChange();

                await NavigateTo(gc);

                break;
            case "welcomeuser":

                var wu = new WelcomeUser();

                await NavigateTo(wu);

                break;
            case "linkreplace":

                var lr = new LinkReplaceTest();

                await NavigateTo(lr);

                break;
        }
    }

    public override async Task OnMemberChanges(MemberChangeEventArgs e)
    {
        if (e.Type == MessageType.ChatMembersAdded)
        {
            await Device.Send("Welcome you new members!\r\n\r\n" + e.Members.Select(a => a.FirstName + " " + a.LastName)
                                                                    .Aggregate((a, b) => a + "\r\n" + b));
        }
        else if (e.Type == MessageType.ChatMemberLeft)
        {
            await Device.Send(
                e.Members.Select(a => a.FirstName + " " + a.LastName).Aggregate((a, b) => a + " and " + b) +
                " has left the group");
        }
    }
}