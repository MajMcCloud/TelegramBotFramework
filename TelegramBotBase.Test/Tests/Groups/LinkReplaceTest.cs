using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TelegramBotBase.Example.Tests.Groups;

public class LinkReplaceTest : GroupForm
{
    private const int Maximum = 3;


    public LinkReplaceTest()
    {
        Opened += LinkReplaceTest_Opened;
    }

    private Dictionary<long, int> Counter { get; } = new();

    private async Task LinkReplaceTest_Opened(object sender, EventArgs e)
    {
        var bf = new ButtonForm();

        bf.AddButtonRow(new ButtonBase("Open GroupChange Test", "groupchange"));
        bf.AddButtonRow(new ButtonBase("Open WelcomeUser Test", "welcomeuser"));
        bf.AddButtonRow(new ButtonBase("Open LinkReplace Test", "linkreplace"));

        await Device.Send("LinkReplaceTest started, click to switch", bf);
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

    public override async Task OnMessage(MessageResult e)
    {
        var from = e.Message.From.Id;

        if (e.Message.From.IsBot)
        {
            return;
        }

        //Are urls inside his message ?
        if (!HasLinks(e.MessageText))
        {
            return;
        }

        var u = await Device.GetChatUser(from);

        //Don't kick Admins or Creators
        if ((u.Status == ChatMemberStatus.Administrator) | (u.Status == ChatMemberStatus.Creator))
        {
            await Device.Send("You won't get kicked,...not this time.");
            return;
        }

        await e.Device.DeleteMessage(e.MessageId);

        var cp = new ChatPermissions
        {
            CanAddWebPagePreviews = false,
            CanChangeInfo = false,
            CanInviteUsers = false,
            CanPinMessages = false,
            CanManageTopics = false,
            CanSendAudios = false,
            CanSendVideos = false,
            CanSendDocuments = false,
            CanSendPhotos = false,
            CanSendVideoNotes = false,
            CanSendVoiceNotes = false,
            CanSendMessages = false,
            CanSendOtherMessages = false,
            CanSendPolls = false
        };

        //Collect user "mistakes" with sending url, after 3 he gets kicked out.
        if (Counter.ContainsKey(from))
        {
            Counter[from]++;
        }
        else
        {
            Counter[from] = 1;
        }


        if (Counter[from] >= 3)
        {
            await e.Device.BanUser(from);

            await e.Device.Send(e.Message.From.FirstName + " " + e.Message.From.LastName +
                                " has been removed from the group");
        }
        else
        {
            await e.Device.RestrictUser(from, cp, null, DateTime.UtcNow.AddSeconds(30));

            await e.Device.Send(e.Message.From.FirstName + " " + e.Message.From.LastName +
                                " has been blocked for 30 seconds");
        }
    }

    /// <summary>
    ///     Call onmessage also on edited message, if someone want to spoof a failed message and insert a link.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public override async Task OnMessageEdit(MessageResult message)
    {
        await OnMessage(message);
    }

    /// <summary>
    ///     https://stackoverflow.com/a/20651284
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private bool HasLinks(string str)
    {
        var tmp = str;

        var pattern =
            @"^(http|https|ftp|)\://|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
        var r = new Regex(pattern);

        var matches = r.Matches(tmp);

        return matches.Count > 0;
    }
}
