using System.Collections.Generic;
using System.Linq;

namespace TelegramBotBase.Base;

public class StateContainer
{
    public StateContainer()
    {
        States = new List<StateEntry>();
    }

    public List<StateEntry> States { get; set; }

    public List<long> ChatIds
    {
        get { return States.Where(a => a.DeviceId > 0).Select(a => a.DeviceId).ToList(); }
    }

    public List<long> GroupIds
    {
        get { return States.Where(a => a.DeviceId < 0).Select(a => a.DeviceId).ToList(); }
    }
}