using TelegramBotBase.Base;

namespace TelegramBotBase.Args
{
    public class SaveStatesEventArgs
    {
        public StateContainer States { get; set; }


        public SaveStatesEventArgs(StateContainer states)
        {
            States = states;
        }
    }
}
