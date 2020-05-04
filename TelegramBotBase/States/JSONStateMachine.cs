using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using System.Text;
using TelegramBotBase.Args;
using TelegramBotBase.Base;
using TelegramBotBase.Form;
using TelegramBotBase.Interfaces;

namespace TelegramBotBase.States
{
    /// <summary>
    /// Is used for all complex data types. Use if other default machines are not working.
    /// </summary>
    public class JSONStateMachine : IStateMachine
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
        public JSONStateMachine(String file, Type fallbackStateForm = null, bool overwrite = true)
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
                var content = System.IO.File.ReadAllText(FilePath);

                var sc = Newtonsoft.Json.JsonConvert.DeserializeObject<StateContainer>(content, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
                }) as StateContainer;

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
                var content = Newtonsoft.Json.JsonConvert.SerializeObject(e.States, Formatting.Indented, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
                });

                System.IO.File.WriteAllText(FilePath, content);
            }
            catch
            {

            }

        }
    }
}
