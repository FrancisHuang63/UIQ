using System.Globalization;

namespace UIQ
{
    public static class DateTimeExtensions
    {
        public static int GetUnixTimestamp(this DateTime dateTime)
        {
            return (int)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static string GetCstUsAbbreviatedDateTime(this DateTime dateTime)
        {
            var targetZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
            var cstDateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime.ToUniversalTime(), targetZone);

            var dateTimeFormatInfo = new CultureInfo("en-US", false).DateTimeFormat;
            return $"{dateTimeFormatInfo.GetAbbreviatedDayName(cstDateTime.DayOfWeek)} {dateTimeFormatInfo.GetAbbreviatedMonthName(cstDateTime.Month)} {cstDateTime.Day} {cstDateTime.ToString("HH:mm:ss")} CST {cstDateTime.Year}";
        }
    }
}