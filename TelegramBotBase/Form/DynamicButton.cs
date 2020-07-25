using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotBase.Form
{
    public class DynamicButton : ButtonBase
    {
        public override string Text
        {
            get
            {
                return GetText?.Invoke() ?? m_text;
            }
            set
            {
                m_text = value;
            }
        }

        private String m_text = "";

        private Func<String> GetText;

        public DynamicButton(String Text, String Value, String Url = null)
        {
            this.Text = Text;
            this.Value = Value;
            this.Url = Url;
        }

        public DynamicButton(Func<String> GetText, String Value, String Url = null)
        {
            this.GetText = GetText;
            this.Value = Value;
            this.Url = Url;
        }


    }
}
