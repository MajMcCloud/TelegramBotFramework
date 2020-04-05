using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System.Text;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.States
{
    /// <summary>
    /// Is used for simple object structures like classes, lists or basic datatypes without generics and other compiler based data types.
    /// </summary>
    public class SimpleJSONStateMachine : IStateMachine
    {
        public String FilePath { get; set; }

        public bool Overwrite { get; set; }

        public SimpleJSONStateMachine(String file, bool overwrite = true)
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
                var content = System.IO.File.ReadAllText(FilePath);

                var sc = Newtonsoft.Json.JsonConvert.DeserializeObject<StateContainer>(content) as StateContainer;

                return sc;
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
                var content = Newtonsoft.Json.JsonConvert.SerializeObject(e.States, Formatting.Indented);

                System.IO.File.WriteAllText(FilePath, content);
            }
            catch
            {

            }

        }
    }
}
