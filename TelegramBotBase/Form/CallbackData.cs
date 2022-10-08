using Newtonsoft.Json;

namespace TelegramBotBase.Form
{
    /// <summary>
    /// Base class for serializing buttons and data
    /// </summary>
    public class CallbackData
    {
        [JsonProperty("m")]
        public string Method { get; set; }

        [JsonProperty("v")]
        public string Value { get; set; }


        public CallbackData()
        {

        }

        public CallbackData(string method, string value)
        {
            Method = method;
            Value = value;
        }

        public static string Create(string method, string value)
        {
            return new CallbackData(method, value).Serialize();
        }

        /// <summary>
        /// Serializes data to json string
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            var s = "";
            try
            {

                s = JsonConvert.SerializeObject(this);
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
        public static CallbackData Deserialize(string data)
        {
            CallbackData cd = null;
            try
            {
                cd = JsonConvert.DeserializeObject<CallbackData>(data);

                return cd;
            }
            catch
            {

            }

            return null;
        }

    }
}
