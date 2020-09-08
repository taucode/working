using System;

namespace TauCode.Working.Tests
{
    internal static class TestHelper
    {
        internal static DateTime TruncateMilliseconds(this DateTime dateTime)
        {
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                dateTime.Hour,
                dateTime.Minute,
                dateTime.Second,
                0,
                dateTime.Kind);
        }
    }
}
