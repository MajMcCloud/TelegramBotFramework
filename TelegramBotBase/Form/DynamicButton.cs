using System;

namespace TelegramBotBase.Form
{
    public class DynamicButton : ButtonBase
    {
        public override string Text
        {
            get => _getText?.Invoke() ?? _mText;
            set => _mText = value;
        }

        private string _mText = "";

        private Func<string> _getText;

        public DynamicButton(string text, string value, string url = null)
        {
            this.Text = text;
            this.Value = value;
            this.Url = url;
        }

        public DynamicButton(Func<string> getText, string value, string url = null)
        {
            this._getText = getText;
            this.Value = value;
            this.Url = url;
        }


    }
}
