using System;

namespace TauCode.Working.Schedules
{
    public interface ISchedule
    {
        string Description { get; set; }
        DateTimeOffset GetDueTimeAfter(DateTimeOffset after);
    }
}
