using System;

namespace TauCode.Working.Jobs
{
    public interface ISchedule
    {
        string Description { get; set; }
        DateTime GetDueTimeAfter(DateTime after);
    }
}
