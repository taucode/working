using System;

namespace TauCode.Extensions.Lab
{
    public readonly struct DateTimeInterval
    {
        public DateTimeInterval(DateTime from, DateTime to)
        {
            if (from.Kind != to.Kind)
            {
                throw new ArgumentException($"'{nameof(from)}' and '{nameof(to)}' must be of same kind.");
            }

            if (from > to)
            {
                throw new ArgumentException($"'{nameof(from)}' must be not greater than '{nameof(to)}'.");
            }

            this.From = from;
            this.To = to;
        }

        public DateTime From { get; }
        public DateTime To { get; }
    }
}
