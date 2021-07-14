using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TelegramBotBase.Args
{
    public class LoadStateEventArgs
    {
        public Dictionary<String,object> Values { get; set; }

        public LoadStateEventArgs()
        {
            Values = new Dictionary<string, object>();
        }

        public List<String> Keys
        {
            get
            {
                return Values.Keys.ToList();
            }
        }

        public String Get(String key)
        {
            return Values[key].ToString();
        }

        public int GetInt(String key)
        {
            int i = 0;
            if (int.TryParse(Values[key].ToString(), out i))
                return i;

            return 0;
        }

        public double GetDouble(String key)
        {
            double d = 0;
            if (double.TryParse(Values[key].ToString(), out d))
                return d;

            return 0;
        }

        public bool GetBool(String key)
        {
            bool b = false;
            if (bool.TryParse(Values[key].ToString(), out b))
                return b;

            return false;
        }

        public object GetObject(String key)
        {
            return Values[key];
        }

    }
}
