using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotBase.Localizations
{
    public class English : Localization
    {
        public English() : base()
        {
            Values["Language"] = "English";
            Values["ButtonGrid_Title"] = "Menu";
            Values["ButtonGrid_NoItems"] = "There are no items.";
            Values["ButtonGrid_PreviousPage"] = "◀️";
            Values["ButtonGrid_NextPage"] = "▶️";
            Values["ButtonGrid_CurrentPage"] = "Page {0} of {1}";
            Values["ButtonGrid_SearchFeature"] = "💡 Send a message to filter the list. Click the 🔍 to reset the filter.";
            Values["ButtonGrid_Back"] = "Back";
            Values["ButtonGrid_CheckAll"] = "Check all";
            Values["ButtonGrid_UncheckAll"] = "Uncheck all";
            Values["CalendarPicker_Title"] = "Pick date";
            Values["CalendarPicker_PreviousPage"] = "◀️";
            Values["CalendarPicker_NextPage"] = "▶️";
            Values["TreeView_Title"] = "Select node";
            Values["TreeView_LevelUp"] = "🔼 level up";
            Values["ToggleButton_On"] = "On";
            Values["ToggleButton_Off"] = "Off";
            Values["ToggleButton_OnIcon"] = "⚫";
            Values["ToggleButton_OffIcon"] = "⚪";
            Values["ToggleButton_Title"] = "Toggle";
            Values["ToggleButton_Changed"] = "Chosen";
            Values["MultiToggleButton_SelectedIcon"] = "✅";
            Values["MultiToggleButton_Title"] = "Multi-Toggle";
            Values["MultiToggleButton_Changed"] = "Chosen";
            Values["PromptDialog_Back"] = "Back";
            Values["ToggleButton_Changed"] = "Setting changed";
        }


    }
}
