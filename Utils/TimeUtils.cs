using System;

namespace SchoolERP.Api.Utils
{
    public static class TimeUtils
    {
        private static readonly TimeZoneInfo IndiaStandardTime = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public static DateTime GetIstTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IndiaStandardTime);
        }

        public static DateTime ConvertToIst(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(dateTime, IndiaStandardTime);
            }
            // If it's Unspecified or Local, we assume it's UTC representation sent from frontend without 'Z'
            // or we convert it directly. Best is to force it to UTC then to IST.
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc), IndiaStandardTime);
        }
    }
}
