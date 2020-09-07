using System;

namespace TauCode.Working.Scheduling
{
    public interface ISchedule
    {
        string Description { get; set; }
        DateTime GetDueTimeAfter(DateTime after);
    }
}
