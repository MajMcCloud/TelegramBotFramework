using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TelegramBotBase.Form
{
    /// <summary>
    /// Base class for serializing buttons and data
    /// </summary>
    public class CallbackData
    {
        [JsonProperty("m")]
        public String Method { get; set; }

        [JsonProperty("v")]
        public String Value { get; set; }


        public CallbackData()
        {

        }

        public CallbackData(String method, String value)
        {
            this.Method = method;
            this.Value = value;
        }

        public static String Create(String method, String value)
        {
            return new CallbackData(method, value).Serialize();
        }

        /// <summary>
        /// Serializes data to json string
        /// </summary>
        /// <returns></returns>
        public String Serialize()
        {
            String s = "";
            try
            {

                s = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            }
            catch
            {


            }
            return s;
        }

        /// <summary>
        /// Deserializes data from json string
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static CallbackData Deserialize(String data)
        {
            CallbackData cd = null;
            try
            {
                cd = Newtonsoft.Json.JsonConvert.DeserializeObject<CallbackData>(data);

                return cd;
            }
            catch
            {

            }

            return null;
        }

    }
}
