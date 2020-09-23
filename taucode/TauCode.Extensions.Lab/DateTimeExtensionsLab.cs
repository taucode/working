using System;

namespace TauCode.Extensions.Lab
{
    // todo: rename to DateTimeOffsetExtensionsLab
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

        public static DateTimeOffset Min(DateTimeOffset v1, DateTimeOffset v2)
        {
            if (v1 < v2)
            {
                return v1;
            }

            return v2;
        }

        public static DateTimeOffset Max(DateTimeOffset v1, DateTimeOffset v2)
        {
            if (v1 > v2)
            {
                return v1;
            }

            return v2;
        }

        public static TimeSpan Min(TimeSpan v1, TimeSpan v2)
        {
            if (v1 < v2)
            {
                return v1;
            }

            return v2;
        }

        public static TimeSpan Max(TimeSpan v1, TimeSpan v2)
        {
            if (v1 > v2)
            {
                return v1;
            }

            return v2;
        }

        public static TimeSpan MinMax(
            TimeSpan min,
            TimeSpan max,
            TimeSpan v)
        {
            if (min > max)
            {
                throw new ArgumentOutOfRangeException(nameof(max));
            }

            if (v <= min)
            {
                v = min;
            }

            if (v >= max)
            {
                v = max;
            }

            return v;
        }
    }
}
