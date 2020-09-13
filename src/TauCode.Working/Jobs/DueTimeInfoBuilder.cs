using System;
using TauCode.Working.Schedules;

namespace TauCode.Working.Jobs
{
    internal class DueTimeInfoBuilder
    {
        internal void UpdateBySchedule(ISchedule schedule, DateTimeOffset currentTime)
        {
            Type = DueTimeType.BySchedule;
            var dueTime = schedule.GetDueTimeAfter(currentTime);
            DueTime = dueTime;
        }

        internal void UpdateManually(DateTimeOffset manualDueTime)
        {
            Type = DueTimeType.Overridden;
            DueTime = manualDueTime;
        }

        internal DateTimeOffset DueTime { get; private set; }

        internal DueTimeType Type { get; private set; }

        internal DueTimeInfo Build() => new DueTimeInfo(Type, DueTime);
    }
}
