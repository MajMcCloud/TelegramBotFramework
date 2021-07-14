using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Enums;
using TelegramBotBase.Form;
using TelegramBotBase.Tools;
using static TelegramBotBase.Tools.Arrays;
using static TelegramBotBase.Tools.Time;

namespace TelegramBotBase.Controls.Inline
{
    public class CalendarPicker : Base.ControlBase
    {

        public DateTime SelectedDate { get; set; }

        public DateTime VisibleMonth { get; set; }

        public DayOfWeek FirstDayOfWeek { get; set; }

        public CultureInfo Culture { get; set; }


        private int? MessageId { get; set; }

        public String Title { get; set; } = Localizations.Default.Language["CalendarPicker_Title"];

        public eMonthPickerMode PickerMode { get; set; }

        public bool EnableDayView { get; set; } = true;

        public bool EnableMonthView { get; set; } = true;

        public bool EnableYearView { get; set; } = true;
        
        public CalendarPicker(CultureInfo culture)
        {
            this.SelectedDate = DateTime.Today;
            this.VisibleMonth = DateTime.Today;
            this.FirstDayOfWeek = DayOfWeek.Monday;
            this.Culture = culture;
            this.PickerMode = eMonthPickerMode.day;
        }
        
        public CalendarPicker() : this(new CultureInfo("en-en")) { }




        public override async Task Action(MessageResult result, String value = null)
        {
            await result.ConfirmAction();

            switch (result.RawData)
            {
                case "$next$":

                    switch (this.PickerMode)
                    {
                        case eMonthPickerMode.day:
                            this.VisibleMonth = this.VisibleMonth.AddMonths(1);
                            break;

                        case eMonthPickerMode.month:
                            this.VisibleMonth = this.VisibleMonth.AddYears(1);
                            break;

                        case eMonthPickerMode.year:
                            this.VisibleMonth = this.VisibleMonth.AddYears(10);
                            break;
                    }


                    break;
                case "$prev$":

                    switch (this.PickerMode)
                    {
                        case eMonthPickerMode.day:
                            this.VisibleMonth = this.VisibleMonth.AddMonths(-1);
                            break;

                        case eMonthPickerMode.month:
                            this.VisibleMonth = this.VisibleMonth.AddYears(-1);
                            break;

                        case eMonthPickerMode.year:
                            this.VisibleMonth = this.VisibleMonth.AddYears(-10);
                            break;
                    }

                    break;

                case "$monthtitle$":

                    if (this.EnableMonthView)
                    {
                        this.PickerMode = eMonthPickerMode.month;
                    }

                    break;

                case "$yeartitle$":

                    if (this.EnableYearView)
                    {
                        this.PickerMode = eMonthPickerMode.year;
                    }

                    break;
                case "$yearstitle$":

                    if (this.EnableMonthView)
                    {
                        this.PickerMode = eMonthPickerMode.month;
                    }

                    this.VisibleMonth = this.SelectedDate;

                    break;

                default:

                    int day = 0;
                    if (result.RawData.StartsWith("d-") && TryParseDay(result.RawData.Split('-')[1], this.SelectedDate, out day))
                    {
                        this.SelectedDate = new DateTime(this.VisibleMonth.Year, this.VisibleMonth.Month, day);
                    }

                    int month = 0;
                    if (result.RawData.StartsWith("m-") && TryParseMonth(result.RawData.Split('-')[1], out month))
                    {
                        this.SelectedDate = new DateTime(this.VisibleMonth.Year, month, 1);
                        this.VisibleMonth = this.SelectedDate;

                        if (this.EnableDayView)
                        {
                            this.PickerMode = eMonthPickerMode.day;
                        }
                    }

                    int year = 0;
                    if (result.RawData.StartsWith("y-") && TryParseYear(result.RawData.Split('-')[1], out year))
                    {
                        this.SelectedDate = new DateTime(year, SelectedDate.Month, SelectedDate.Day);
                        this.VisibleMonth = this.SelectedDate;

                        if (this.EnableMonthView)
                        {
                            this.PickerMode = eMonthPickerMode.month;
                        }

                    }

                    break;
            }



        }



        public override async Task Render(MessageResult result)
        {



            ButtonForm bf = new ButtonForm();

            switch (this.PickerMode)
            {
                case eMonthPickerMode.day:

                    var month = this.VisibleMonth;

                    string[] dayNamesNormal = this.Culture.DateTimeFormat.ShortestDayNames;
                    string[] dayNamesShifted = Shift(dayNamesNormal, (int)this.FirstDayOfWeek);

                    bf.AddButtonRow(new ButtonBase(Localizations.Default.Language["CalendarPicker_PreviousPage"], "$prev$"), new ButtonBase(this.Culture.DateTimeFormat.MonthNames[month.Month - 1] + " " + month.Year.ToString(), "$monthtitle$"), new ButtonBase(Localizations.Default.Language["CalendarPicker_NextPage"], "$next$"));

                    bf.AddButtonRow(dayNamesShifted.Select(a => new ButtonBase(a, a)).ToList());

                    //First Day of month
                    var firstDay = new DateTime(month.Year, month.Month, 1);

                    //Last Day of month
                    var lastDay = firstDay.LastDayOfMonth();

                    //Start of Week where first day of month is (left border)
                    var start = firstDay.StartOfWeek(this.FirstDayOfWeek);

                    //End of week where last day of month is (right border)
                    var end = lastDay.EndOfWeek(this.FirstDayOfWeek);

                    for (int i = 0; i <= ((end - start).Days / 7); i++)
                    {
                        var lst = new List<ButtonBase>();
                        for (int id = 0; id < 7; id++)
                        {
                            var d = start.AddDays((i * 7) + id);
                            if (d < firstDay | d > lastDay)
                            {
                                lst.Add(new ButtonBase("-", "m-" + d.Day.ToString()));
                                continue;
                            }

                            var day = d.Day.ToString();

                            if (d == DateTime.Today)
                            {
                                day = "(" + day + ")";
                            }

                            lst.Add(new ButtonBase((this.SelectedDate == d ? "[" + day + "]" : day), "d-" + d.Day.ToString()));
                        }
                        bf.AddButtonRow(lst);
                    }

                    break;

                case eMonthPickerMode.month:

                    bf.AddButtonRow(new ButtonBase(Localizations.Default.Language["CalendarPicker_PreviousPage"], "$prev$"), new ButtonBase(this.VisibleMonth.Year.ToString("0000"), "$yeartitle$"), new ButtonBase(Localizations.Default.Language["CalendarPicker_NextPage"], "$next$"));

                    var months = this.Culture.DateTimeFormat.MonthNames;

                    var buttons = months.Select((a, b) => new ButtonBase((b == this.SelectedDate.Month - 1 && this.SelectedDate.Year == this.VisibleMonth.Year ? "[ " + a + " ]" : a), "m-" + (b + 1).ToString()));

                    bf.AddSplitted(buttons, 2);

                    break;

                case eMonthPickerMode.year:

                    bf.AddButtonRow(new ButtonBase(Localizations.Default.Language["CalendarPicker_PreviousPage"], "$prev$"), new ButtonBase("Year", "$yearstitle$"), new ButtonBase(Localizations.Default.Language["CalendarPicker_NextPage"], "$next$"));

                    var starti = Math.Floor(this.VisibleMonth.Year / 10f) * 10;

                    for (int i = 0; i < 10; i++)
                    {
                        var m = starti + (i * 2);
                        bf.AddButtonRow(new ButtonBase((this.SelectedDate.Year == m ? "[ " + m.ToString() + " ]" : m.ToString()), "y-" + m.ToString()), new ButtonBase((this.SelectedDate.Year == (m + 1) ? "[ " + (m + 1).ToString() + " ]" : (m + 1).ToString()), "y-" + (m + 1).ToString()));
                    }

                    break;

            }


            if (this.MessageId != null)
            {
                var m = await this.Device.Edit(this.MessageId.Value, this.Title, bf);
            }
            else
            {
                var m = await this.Device.Send(this.Title, bf);
                this.MessageId = m.MessageId;
            }
        }



        public override async Task Cleanup()
        {

            if (this.MessageId != null)
            {
                await this.Device.DeleteMessage(this.MessageId.Value);
            }

        }

    }
}
