using System;

namespace TauCode.Extensions.Lab
{
    public static class DateTimeExtensionsLab
    {
        public static DateTimeOffset ToUtcDayOffset(this string timeString)
        {
            var time = DateTimeOffset.Parse(timeString);
            if (time.Offset != TimeSpan.Zero)
            {
                throw new ArgumentException($"'{timeString}' does not represent a UTC date with zero day time.", nameof(timeString));
            }

            return time;
        }
    }
}
