using System.Collections.Generic;

namespace TelegramBotBase.Args;

public class SaveStateEventArgs
{
    public SaveStateEventArgs()
    {
        Values = new Dictionary<string, object>();
    }

    public Dictionary<string, object> Values { get; set; }

    public void Set(string key, string value)
    {
        Values[key] = value;
    }

    public void SetInt(string key, int value)
    {
        Values[key] = value;
    }

    public void SetBool(string key, bool value)
    {
        Values[key] = value;
    }

    public void SetDouble(string key, double value)
    {
        Values[key] = value;
    }

    public void SetObject(string key, object value)
    {
        Values[key] = value;
    }
}