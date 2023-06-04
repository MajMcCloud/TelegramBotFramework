using TelegramBotBase.Base;

namespace TelegramBotBase.Args;

public class SaveStatesEventArgs
{
    public SaveStatesEventArgs(StateContainer states)
    {
        States = states;
    }

    public StateContainer States { get; set; }
}