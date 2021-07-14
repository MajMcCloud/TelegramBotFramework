using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotBase.Tools
{
    public static class Time
    {
        public static bool TryParseDay(string src, DateTime currentDate, out int resultDay)
        {
            return int.TryParse(src, out resultDay) && resultDay >= 1 && resultDay <= DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
        }

        public static bool TryParseMonth(string src, out int resultMonth)
        {
            return int.TryParse(src, out resultMonth) && resultMonth >= 1 && resultMonth <= 12;
        }

        public static bool TryParseYear(string src, out int resultYear)
        {
            return int.TryParse(src, out resultYear) && resultYear >= 0 && resultYear <= DateTime.MaxValue.Year;
        }
        
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }
            return dt.AddDays(-1 * diff).Date;
        }

        public static DateTime EndOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            return StartOfWeek(dt, startOfWeek).AddDays(6);
        }

        public static DateTime FirstDayOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime LastDayOfMonth(this DateTime date)
        {
            return FirstDayOfMonth(date).AddMonths(1).AddDays(-1);
        }
    }
}
