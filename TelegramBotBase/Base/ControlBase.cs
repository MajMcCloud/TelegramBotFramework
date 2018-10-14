using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotBase.Base
{
    public class ControlBase
    {
        public Sessions.DeviceSession Device { get; set; }

        public virtual async Task Render()
        {

        }

        public virtual async Task Cleanup()
        {

        }

    }
}
