using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.States
{
    public class XMLStateMachine : IStateMachine
    {
        public String FilePath { get; set; }

        public bool Overwrite { get; set; }

        public XMLStateMachine(String file, bool overwrite = true)
        {
            if (file is null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            this.FilePath = file;
            this.Overwrite = overwrite;
        }

        public StateContainer LoadFormStates()
        {
            try
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(StateContainer));

                using (var reader = new StreamReader(FilePath))
                {
                    using (var xml = new XmlTextReader(reader))
                    {
                        StateContainer sc = serializer.ReadObject(xml) as StateContainer;
                        return sc;
                    }
                }
            }
            catch
            {

            }

            return new StateContainer();
        }

        public void SaveFormStates(SaveStatesEventArgs e)
        {
            if (System.IO.File.Exists(FilePath))
            {
                if (!this.Overwrite)
                {
                    throw new Exception("File exists already.");
                }

                System.IO.File.Delete(FilePath);
            }

            try
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(StateContainer));

                using (var sw = new StreamWriter(this.FilePath))
                {
                    using (var writer = new XmlTextWriter(sw))
                    {
                        writer.Formatting = Formatting.Indented; // indent the Xml so it’s human readable
                        serializer.WriteObject(writer, e.States);
                        writer.Flush();
                    }
                }
            }
            catch
            {

            }

        }
    }

}
