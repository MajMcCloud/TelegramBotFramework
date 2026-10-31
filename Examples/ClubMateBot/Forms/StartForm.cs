// ClubMateBot demo - SKILL.md 13 (SplitterForm): routes an update by chat type.
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace ClubMateBot.Forms;

/// <summary>
///     Entry point of the bot. A <see cref="SplitterForm" /> never renders anything itself -
///     it only decides which form takes over for the current chat.
///     Returning true means "handled, stop here"; returning false lets the splitter fall through
///     to <see cref="SplitterForm.Open" />.
/// </summary>
public sealed class StartForm : SplitterForm
{
    /// <summary>Private chat: the feature catalogue.</summary>
    public override async Task<bool> Open(MessageResult e)
    {
        await NavigateTo(new MenuForm());

        return true;
    }

    /// <summary>Basic group. Phase M5 replaces the info message with GroupManageForm.</summary>
    public override async Task<bool> OpenGroup(MessageResult e)
    {
        await Device.Send(
            "Group moderation (GroupForm, join/leave handling, bans) is added in phase M5 - " +
            "add me to a group and use /start there once it is implemented.");

        return true;
    }

    public override async Task<bool> OpenSupergroup(MessageResult e)
    {
        return await OpenGroup(e);
    }

    public override async Task<bool> OpenChannel(MessageResult e)
    {
        await Device.Send("Channel support (broadcast + admin commands) is part of phase M5 as well.");

        return true;
    }
}
