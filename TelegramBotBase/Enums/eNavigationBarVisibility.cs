using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotBase.Enums
{
    public enum eNavigationBarVisibility
    {
        /// <summary>
        /// Shows it depending on the amount of items.
        /// </summary>
        auto = 0,

        /// <summary>
        /// Will not show it at any time.
        /// </summary>
        never = 1,

        /// <summary>
        /// Will show it at any time.
        /// </summary>
        always = 2

    }
}
