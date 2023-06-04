using System;
using System.Threading.Tasks;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests.Groups;

public class GroupChange : GroupForm
{
    public GroupChange()
    {
        Opened += GroupChange_Opened;
    }


    private async Task GroupChange_Opened(object sender, EventArgs e)
    {
        var bf = new ButtonForm();

        bf.AddButtonRow(new ButtonBase("Open GroupChange Test", "groupchange"));
        bf.AddButtonRow(new ButtonBase("Open WelcomeUser Test", "welcomeuser"));
        bf.AddButtonRow(new ButtonBase("Open LinkReplace Test", "linkreplace"));

        await Device.Send("GroupChange started, click to switch", bf);
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

    public override async Task OnGroupChanged(GroupChangedEventArgs e)
    {
        await Device.Send("Group has been changed by " + e.OriginalMessage.Message.From.FirstName + " " +
                          e.OriginalMessage.Message.From.LastName);
    }
}