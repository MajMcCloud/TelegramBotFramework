using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotBase.Args
{
    public class SaveStateEventArgs
    {
        public Dictionary<String, object> Values { get; set; }

        public SaveStateEventArgs()
        {
            Values = new Dictionary<string, object>();
        }

        public void Set(String key, String value)
        {
            Values[key] = value;
        }

        public void SetInt(String key, int value)
        {
            Values[key] = value;
        }

        public void SetBool(String key, bool value)
        {
            Values[key] = value;
        }

        public void SetDouble(String key, double value)
        {
            Values[key] = value;
        }
        public void SetObject(String key, object value)
        {
            Values[key] = value;
        }

    }
}
