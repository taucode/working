using System;

namespace TauCode.Extensions.Lab
{
    public readonly struct DateTimeInterval
    {
        public DateTimeInterval(DateTime start, DateTime end)
        {
            if (start.Kind != end.Kind)
            {
                throw new ArgumentException($"'{nameof(start)}' and '{nameof(end)}' must be of same kind.");
            }

            if (start > end)
            {
                throw new ArgumentException($"'{nameof(start)}' must be not greater than '{nameof(end)}'.");
            }

            this.Start = start;
            this.End = end;
        }

        public DateTime Start { get; }
        public DateTime End { get; }
    }
}
