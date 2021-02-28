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

        public int ID { get; set; }

        public String ControlID
        {
            get
            {
                return "#c" + this.ID.ToString();
            }
        }

        /// <summary>
        /// Defines if the control should be rendered and invoked with actions
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Get invoked when control will be added to a form and invoked.
        /// </summary>
        /// <returns></returns>
        public virtual void Init()
        {

        }

        public virtual async Task Load(MessageResult result)
        {


            
        }

        public virtual async Task Action(MessageResult result, String value = null)
        {


            
        }



        public virtual async Task Render(MessageResult result)
        {



            
        }

        public virtual async Task Hidden(bool FormClose)
        {

            
        }

        /// <summary>
        /// Will be called on a cleanup.
        /// </summary>
        /// <returns></returns>
        public virtual async Task Cleanup()
        {
            
        }

    }
}
