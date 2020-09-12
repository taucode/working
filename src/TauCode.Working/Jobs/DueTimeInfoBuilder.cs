using System;

namespace TauCode.Working.Jobs
{
    internal class DueTimeInfoBuilder
    {
        internal void UpdateBySchedule(ISchedule schedule, DateTime currentTime)
        {
            Type = DueTimeType.BySchedule;
            var dueTime = schedule.GetDueTimeAfter(currentTime);
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
