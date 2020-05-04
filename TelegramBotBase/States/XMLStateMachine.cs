using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.States
{
    public class XMLStateMachine : IStateMachine
    {
        public String FilePath { get; set; }

        public bool Overwrite { get; set; }

        public Type FallbackStateForm { get; private set; }

        /// <summary>
        /// Will initialize the state machine.
        /// </summary>
        /// <param name="file">Path of the file and name where to save the session details.</param>
        /// <param name="fallbackStateForm">Type of Form which will be saved instead of Form which has <seealso cref="Attributes.IgnoreState"/> attribute declared. Needs to be subclass of <seealso cref="Form.FormBase"/>.</param>
        /// <param name="overwrite">Declares of the file could be overwritten.</param>
        public XMLStateMachine(String file, Type fallbackStateForm = null, bool overwrite = true)
        {
            if (file is null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            this.FallbackStateForm = fallbackStateForm;

            if (this.FallbackStateForm != null && !this.FallbackStateForm.IsSubclassOf(typeof(FormBase)))
            {
                throw new ArgumentException("FallbackStateForm is not a subclass of FormBase");
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
