using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotBase.Form
{
    /// <summary>
    /// Base class for serializing buttons and data
    /// </summary>
    public class CallbackData
    {
        public String Method { get; set; }

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
