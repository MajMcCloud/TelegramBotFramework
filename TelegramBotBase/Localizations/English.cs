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
            Values["ButtonGrid_CurrentPage"] = "Page {0} of {1}";
            Values["CalendarPicker_Title"] = "Pick date";
            Values["TreeView_Title"] = "Select node";
            Values["TreeView_LevelUp"] = "🔼 level up";
            Values["ToggleButton_On"] = "On";
            Values["ToggleButton_Off"] = "Off";
            Values["ToggleButton_Title"] = "Toggle";
            Values["ToggleButton_Changed"] = "Setting changed";
        }


    }
}
