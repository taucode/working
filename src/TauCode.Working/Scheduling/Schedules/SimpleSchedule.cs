using System;

namespace TauCode.Working.Scheduling.Schedules
{
    public class SimpleSchedule : ISchedule
    {
        public SimpleSchedule(SimpleScheduleKind kind, int multiplier, DateTime baseTime)
        {
            if (baseTime.Kind != DateTimeKind.Utc)
            {
                throw new NotImplementedException(); // todo
            }

            if (baseTime.Millisecond != 0)
            {
                throw new NotImplementedException(); // todo
            }

            if (multiplier <= 0)
            {
                throw new NotImplementedException(); // todo
            }

            this.Kind = kind;
            this.Multiplier = multiplier;
            this.BaseTime = baseTime;

            this.TimeSpan = this.CalculateTimeSpan();
        }

        private TimeSpan CalculateTimeSpan()
        {
            TimeSpan timeSpan;

            switch (this.Kind)
            {
                case SimpleScheduleKind.Second:
                    timeSpan = TimeSpan.FromSeconds(this.Multiplier);
                    break;

                case SimpleScheduleKind.Minute:
                    timeSpan = TimeSpan.FromMinutes(this.Multiplier);
                    break;

                case SimpleScheduleKind.Hour:
                    timeSpan = TimeSpan.FromHours(this.Multiplier);
                    break;

                case SimpleScheduleKind.Day:
                    timeSpan = TimeSpan.FromDays(this.Multiplier);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return timeSpan;
        }

        public SimpleScheduleKind Kind { get; }

        public int Multiplier { get; }

        public DateTime BaseTime { get; }

        public TimeSpan TimeSpan { get; }

        public string Description { get; set; }

        public DateTime GetDueTimeAfter(DateTime after)
        {
            if (after.Kind != DateTimeKind.Utc)
            {
                throw new NotImplementedException(); // todo
            }

            if (after == this.BaseTime)
            {
                return after.Add(this.TimeSpan);
            }
            else if (after > this.BaseTime)
            {
                var spanCount = (int)((after - this.BaseTime).TotalMilliseconds / this.TimeSpan.TotalMilliseconds);
                var result = this.BaseTime.AddMilliseconds(spanCount * this.TimeSpan.TotalMilliseconds);

                while (true)
                {
                    result = result.TruncateMilliseconds();
                    if (result > after)
                    {
                        return result;
                    }

                    result = result.AddMilliseconds(this.TimeSpan.TotalMilliseconds);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
