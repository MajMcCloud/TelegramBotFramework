using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotBase.Localizations
{
    public class Localization
    {
        public Dictionary<String, String> Values = new Dictionary<string, string>();

        public String this[String key]
        {
            get
            {
                return Values[key];
            }
        }

        public Localization()
        {
            Values["Language"] = "Deutsch (German)";
            Values["ButtonGrid_Title"] = "Menü";
            Values["CalendarPicker_Title"] = "Datum auswählen";
            Values["CalendarPicker_PreviousPage"] = "◀️";
            Values["CalendarPicker_NextPage"] = "▶️";
            Values["TreeView_Title"] = "Knoten auswählen";
            Values["TreeView_LevelUp"] = "🔼 Stufe hoch";
            Values["ToggleButton_On"] = "An";
            Values["ToggleButton_Off"] = "Aus";
            Values["ToggleButton_OnIcon"] = "⚫";
            Values["ToggleButton_OffIcon"] = "⚪";
            Values["ToggleButton_Title"] = "Schalter";
            Values["PromptDialog_Back"] = "Zurück";
            
        }

    }
}

