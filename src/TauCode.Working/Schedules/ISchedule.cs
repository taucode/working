using System;

namespace TauCode.Working.Schedules
{
    public interface ISchedule
    {
        string Description { get; set; }
        DateTime GetDueTimeAfter(DateTime after);
    }
}
