using System.Collections.Generic;

namespace TelegramBotBase.Localizations;

public abstract class Localization
{
    public Dictionary<string, string> Values = new();

    public string this[string key] => Values[key];
}