using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;

namespace TelegramBotBase.Base
{
    
    [DebuggerDisplay("Device: {DeviceId}, {FormUri}")]
    public class StateEntry
    {
        /// <summary>
        /// Contains the DeviceId of the entry.
        /// </summary>
        public long DeviceId { get; set; }

        /// <summary>
        /// Contains the Username (on privat chats) or Group title on groups/channels.
        /// </summary>
        public String ChatTitle { get; set; }

        /// <summary>
        /// Contains additional values to save.
        /// </summary>
        public Dictionary<String, object> Values { get; set; }

        /// <summary>
        /// Contains the full qualified namespace of the form to used for reload it via reflection.
        /// </summary>
        public String FormUri {get;set;}

        /// <summary>
        /// Contains the assembly, where to find that form.
        /// </summary>
        public String QualifiedName { get; set; }

        public StateEntry()
        {
            this.Values = new Dictionary<string, object>();
        }

    }
}
