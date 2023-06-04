using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;
using TelegramBotBase.Localizations;
using static TelegramBotBase.Tools.Arrays;
using static TelegramBotBase.Tools.Time;

namespace TelegramBotBase.Controls.Inline;

public class CalendarPicker : ControlBase
{
    public CalendarPicker(CultureInfo culture)
    {
        SelectedDate = DateTime.Today;
        VisibleMonth = DateTime.Today;
        FirstDayOfWeek = DayOfWeek.Monday;
        Culture = culture;
        PickerMode = EMonthPickerMode.day;
    }

    public CalendarPicker() : this(new CultureInfo("en-en"))
    {
    }

    public DateTime SelectedDate { get; set; }

    public DateTime VisibleMonth { get; set; }

    public DayOfWeek FirstDayOfWeek { get; set; }

    public CultureInfo Culture { get; set; }


    public int? MessageId { get; set; }

    public string Title { get; set; } = Default.Language["CalendarPicker_Title"];

    public EMonthPickerMode PickerMode { get; set; }

    public bool EnableDayView { get; set; } = true;

    public bool EnableMonthView { get; set; } = true;

    public bool EnableYearView { get; set; } = true;


    public override async Task Action(MessageResult result, string value = null)
    {
        await result.ConfirmAction();

        switch (result.RawData)
        {
            case "$next$":

                VisibleMonth = PickerMode switch
                {
                    EMonthPickerMode.day => VisibleMonth.AddMonths(1),
                    EMonthPickerMode.month => VisibleMonth.AddYears(1),
                    EMonthPickerMode.year => VisibleMonth.AddYears(10),
                    _ => VisibleMonth
                };

                break;
            case "$prev$":

                VisibleMonth = PickerMode switch
                {
                    EMonthPickerMode.day => VisibleMonth.AddMonths(-1),
                    EMonthPickerMode.month => VisibleMonth.AddYears(-1),
                    EMonthPickerMode.year => VisibleMonth.AddYears(-10),
                    _ => VisibleMonth
                };

                break;

            case "$monthtitle$":

                if (EnableMonthView)
                {
                    PickerMode = EMonthPickerMode.month;
                }

                break;

            case "$yeartitle$":

                if (EnableYearView)
                {
                    PickerMode = EMonthPickerMode.year;
                }

                break;
            case "$yearstitle$":

                if (EnableMonthView)
                {
                    PickerMode = EMonthPickerMode.month;
                }

                VisibleMonth = SelectedDate;

                break;

            default:

                var day = 0;
                if (result.RawData.StartsWith("d-") &&
                    TryParseDay(result.RawData.Split('-')[1], SelectedDate, out day))
                {
                    SelectedDate = new DateTime(VisibleMonth.Year, VisibleMonth.Month, day);
                }

                var month = 0;
                if (result.RawData.StartsWith("m-") && TryParseMonth(result.RawData.Split('-')[1], out month))
                {
                    SelectedDate = new DateTime(VisibleMonth.Year, month, 1);
                    VisibleMonth = SelectedDate;

                    if (EnableDayView)
                    {
                        PickerMode = EMonthPickerMode.day;
                    }
                }

                var year = 0;
                if (result.RawData.StartsWith("y-") && TryParseYear(result.RawData.Split('-')[1], out year))
                {
                    SelectedDate = new DateTime(year, SelectedDate.Month, SelectedDate.Day);
                    VisibleMonth = SelectedDate;

                    if (EnableMonthView)
                    {
                        PickerMode = EMonthPickerMode.month;
                    }
                }

                break;
        }
    }


    public override async Task Render(MessageResult result)
    {
        var bf = new ButtonForm();

        switch (PickerMode)
        {
            case EMonthPickerMode.day:

                var month = VisibleMonth;

                var dayNamesNormal = Culture.DateTimeFormat.ShortestDayNames;
                var dayNamesShifted = Shift(dayNamesNormal, (int)FirstDayOfWeek);

                bf.AddButtonRow(new ButtonBase(Default.Language["CalendarPicker_PreviousPage"], "$prev$"),
                                new ButtonBase(Culture.DateTimeFormat.MonthNames[month.Month - 1] + " " + month.Year,
                                               "$monthtitle$"),
                                new ButtonBase(Default.Language["CalendarPicker_NextPage"], "$next$"));

                bf.AddButtonRow(dayNamesShifted.Select(a => new ButtonBase(a, a)).ToList());

                //First Day of month
                var firstDay = new DateTime(month.Year, month.Month, 1);

                //Last Day of month
                var lastDay = firstDay.LastDayOfMonth();

                //Start of Week where first day of month is (left border)
                var start = firstDay.StartOfWeek(FirstDayOfWeek);

                //End of week where last day of month is (right border)
                var end = lastDay.EndOfWeek(FirstDayOfWeek);

                for (var i = 0; i <= (end - start).Days / 7; i++)
                {
                    var lst = new List<ButtonBase>();
                    for (var id = 0; id < 7; id++)
                    {
                        var d = start.AddDays(i * 7 + id);
                        if ((d < firstDay) | (d > lastDay))
                        {
                            lst.Add(new ButtonBase("-", "m-" + d.Day));
                            continue;
                        }

                        var day = d.Day.ToString();

                        if (d == DateTime.Today)
                        {
                            day = "(" + day + ")";
                        }

                        lst.Add(new ButtonBase(SelectedDate == d ? "[" + day + "]" : day, "d-" + d.Day));
                    }

                    bf.AddButtonRow(lst);
                }

                break;

            case EMonthPickerMode.month:

                bf.AddButtonRow(new ButtonBase(Default.Language["CalendarPicker_PreviousPage"], "$prev$"),
                                new ButtonBase(VisibleMonth.Year.ToString("0000"), "$yeartitle$"),
                                new ButtonBase(Default.Language["CalendarPicker_NextPage"], "$next$"));

                var months = Culture.DateTimeFormat.MonthNames;

                var buttons = months.Select((a, b) =>
                                                new ButtonBase(
                                                    b == SelectedDate.Month - 1 &&
                                                    SelectedDate.Year == VisibleMonth.Year
                                                        ? "[ " + a + " ]"
                                                        : a,
                                                    "m-" + (b + 1)));

                bf.AddSplitted(buttons);

                break;

            case EMonthPickerMode.year:

                bf.AddButtonRow(new ButtonBase(Default.Language["CalendarPicker_PreviousPage"], "$prev$"),
                                new ButtonBase("Year", "$yearstitle$"),
                                new ButtonBase(Default.Language["CalendarPicker_NextPage"], "$next$"));

                var starti = Math.Floor(VisibleMonth.Year / 10f) * 10;

                for (var i = 0; i < 10; i++)
                {
                    var m = starti + i * 2;
                    bf.AddButtonRow(
                        new ButtonBase(SelectedDate.Year == m ? "[ " + m + " ]" : m.ToString(), "y-" + m),
                        new ButtonBase(SelectedDate.Year == m + 1 ? "[ " + (m + 1) + " ]" : (m + 1).ToString(),
                                       "y-" + (m + 1)));
                }

                break;
        }


        if (MessageId != null)
        {
            var m = await Device.Edit(MessageId.Value, Title, bf);
        }
        else
        {
            var m = await Device.Send(Title, bf);
            MessageId = m.MessageId;
        }
    }


    public override async Task Cleanup()
    {
        if (MessageId != null)
        {
            await Device.DeleteMessage(MessageId.Value);
        }
    }
}