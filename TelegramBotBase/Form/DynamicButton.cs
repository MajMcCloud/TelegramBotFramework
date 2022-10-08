using System;

namespace TelegramBotBase.Form;

public class DynamicButton : ButtonBase
{
    private readonly Func<string> _getText;

    private string _mText = "";

    public DynamicButton(string text, string value, string url = null)
    {
        Text = text;
        Value = value;
        Url = url;
    }

    public DynamicButton(Func<string> getText, string value, string url = null)
    {
        _getText = getText;
        Value = value;
        Url = url;
    }

    public override string Text
    {
        get => _getText?.Invoke() ?? _mText;
        set => _mText = value;
    }
}