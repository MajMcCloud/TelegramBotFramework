using TelegramBotBase.Enums;

namespace TelegramBotBase.Controls.Inline;

public class MonthPicker : CalendarPicker
{
    public MonthPicker()
    {
        PickerMode = EMonthPickerMode.month;
        EnableDayView = false;
    }
}