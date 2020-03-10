using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SystemCommandsBot.commands
{
    public class Commando
    {
        public int ID { get; set; }

        public String Title { get; set; }

        public String ShellCmd { get; set; }

        public bool Enabled { get; set; } = true;

        public String Action { get; set; }

        public bool UseShell { get; set; } = true;


        public int? MaxInstances { get; set; }

        public String ProcName
        {
            get;set;
        }
    }
}
