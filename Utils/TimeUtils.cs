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
    }
}
