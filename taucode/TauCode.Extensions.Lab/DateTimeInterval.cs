using System;

namespace TauCode.Extensions.Lab
{
    // todo ut-s
    public readonly struct DateTimeOffsetInterval
    {
        public DateTimeOffsetInterval(DateTimeOffset start, DateTimeOffset end)
        {
            if (start > end)
            {
                throw new ArgumentException($"'{nameof(end)}' must be not earlier than '{nameof(start)}'.", nameof(end));
            }

            this.Start = start;
            this.End = end;
        }

        public DateTimeOffset Start { get; }
        public DateTimeOffset End { get; }

        public static DateTimeOffsetInterval Parse(string intervalString)
        {
            if (intervalString == null)
            {
                throw new ArgumentNullException(nameof(intervalString));
            }

            var parts = intervalString.Split(' ');
            if (parts.Length != 2)
            {
                throw new ArgumentException($"'{nameof(intervalString)}' must be in format '<start> <end>.'");
            }

            var startString = parts[0];
            var endString = parts[1];

            var start = DateTimeOffset.Parse(startString);
            var end = DateTimeOffset.Parse(endString);

            var result = new DateTimeOffsetInterval(start, end);

            return result;
        }
    }
}
