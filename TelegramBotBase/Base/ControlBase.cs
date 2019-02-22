using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotBase.Base
{
    /// <summary>
    /// Base class for controls
    /// </summary>
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
