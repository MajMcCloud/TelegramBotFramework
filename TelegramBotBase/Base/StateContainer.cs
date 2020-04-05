using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TelegramBotBase.Base
{
    public partial class StateContainer
    {
        public List<StateEntry> States { get; set; }

        public List<long> ChatIds
        {
            get
            {
                return States.Where(a => a.DeviceId > 0).Select(a => a.DeviceId).ToList();
            }
        }

        public List<long> GroupIds
        {
            get
            {
                return States.Where(a => a.DeviceId < 0).Select(a => a.DeviceId).ToList();
            }
        }

        public StateContainer()
        {
            this.States = new List<StateEntry>();
        }

    }
}
