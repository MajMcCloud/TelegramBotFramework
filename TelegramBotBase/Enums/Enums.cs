using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotBase.Enums
{
    public enum eDeleteMode
    {
        /// <summary>
        /// Don't delete any message.
        /// </summary>
        None = 0,
        /// <summary>
        /// Delete messages on every callback/action.
        /// </summary>
        OnEveryCall = 1,
        /// <summary>
        /// Delete on leaving this form.
        /// </summary>
        OnLeavingForm = 2
    }

    public enum eSide
    {
        /// <summary>
        /// Delete only messages from this bot.
        /// </summary>
        BotOnly = 0,
        /// <summary>
        /// Delete only user messages.
        /// </summary>
        UserOnly = 1,
        /// <summary>
        /// Delete all messages in this context.
        /// </summary>
        Both = 2
    }

    public enum eMonthPickerMode
    {
        /// <summary>
        /// Shows the calendar with day picker mode
        /// </summary>
        day = 0,
        /// <summary>
        /// Shows the calendar with month overview
        /// </summary>
        month = 1,
        /// <summary>
        /// Shows the calendar with year overview
        /// </summary>
        year = 2
    }
}
