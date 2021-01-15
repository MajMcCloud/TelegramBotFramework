using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;

namespace TelegramBotBase.Controls.Inline
{
    public class MonthPicker : CalendarPicker
    {

    

        public MonthPicker()
        {
            this.PickerMode = eMonthPickerMode.month;
            this.EnableDayView = false;
        }


    }
}
