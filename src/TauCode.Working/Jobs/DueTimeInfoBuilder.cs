using System;
using TauCode.Infrastructure.Time;

namespace TauCode.Working.Jobs
{
    internal class DueTimeInfoBuilder
    {
        internal void UpdateBySchedule(ISchedule schedule)
        {
            Type = DueTimeType.BySchedule;
            var now = TimeProvider.GetCurrent();
            var dueTime = schedule.GetDueTimeAfter(now);
            DueTime = dueTime;
        }

        internal void UpdateManually(DateTime manualDueTime)
        {
            Type = DueTimeType.Overridden;
            DueTime = manualDueTime;
        }

        internal DateTime DueTime { get; private set; }

        public DueTimeType Type { get; private set; }

        internal DueTimeInfo Build() => new DueTimeInfo(Type, DueTime);
    }
}
