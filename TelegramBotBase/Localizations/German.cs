using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotBase.Localizations
{
    public class German : Localization
    {
        public German() : base()
        {
            Values["Language"] = "Deutsch (German)";
            Values["ButtonGrid_Title"] = "Menü";
            Values["ButtonGrid_NoItems"] = "Es sind keine Einträge vorhanden.";
            Values["ButtonGrid_PreviousPage"] = "◀️";
            Values["ButtonGrid_NextPage"] = "▶️";
            Values["ButtonGrid_CurrentPage"] = "Seite {0} von {1}";
            Values["ButtonGrid_SearchFeature"] = "💡 Sende eine Nachricht um die Liste zu filtern. Klicke die 🔍 um den Filter zurückzusetzen.";
            Values["ButtonGrid_Back"] = "Zurück";
            Values["ButtonGrid_CheckAll"] = "Alle auswählen";
            Values["ButtonGrid_UncheckAll"] = "Keine auswählen";
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
            Values["ToggleButton_Changed"] = "Ausgewählt";
            Values["MultiToggleButton_SelectedIcon"] = "✅";
            Values["MultiToggleButton_Title"] = "Mehrfach-Schalter";
            Values["MultiToggleButton_Changed"] = "Ausgewählt";
            Values["PromptDialog_Back"] = "Zurück";
            Values["ToggleButton_Changed"] = "Einstellung geändert";
        }


    }
}
