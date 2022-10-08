using System.Collections.Generic;
using System.Linq;

namespace TelegramBotBase.Args;

public class LoadStateEventArgs
{
    public LoadStateEventArgs()
    {
        Values = new Dictionary<string, object>();
    }

    public Dictionary<string, object> Values { get; set; }

    public List<string> Keys => Values.Keys.ToList();

    public string Get(string key)
    {
        return Values[key].ToString();
    }

    public int GetInt(string key)
    {
        var i = 0;
        if (int.TryParse(Values[key].ToString(), out i))
        {
            return i;
        }

        return 0;
    }

    public double GetDouble(string key)
    {
        double d = 0;
        if (double.TryParse(Values[key].ToString(), out d))
        {
            return d;
        }

        return 0;
    }

    public bool GetBool(string key)
    {
        var b = false;
        if (bool.TryParse(Values[key].ToString(), out b))
        {
            return b;
        }

        return false;
    }

    public object GetObject(string key)
    {
        return Values[key];
    }
}