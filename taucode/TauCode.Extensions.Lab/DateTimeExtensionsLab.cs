using System;

namespace TauCode.Extensions.Lab
{
    public static class DateTimeExtensionsLab
    {
        public static DateTime ToExactUtcDate(this string dateString)
        {
            var date = DateTime.Parse(dateString);

            var result = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            return result;
        }

        public static DateTimeInterval ToExactUtcDateTimeInterval(this string dateIntervalString)
        {
            throw new NotImplementedException();
        }
    }
}
