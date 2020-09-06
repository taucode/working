using System;

namespace TauCode.Working
{
    public interface ISchedule
    {
        DateTime GetNextDueTime();
    }
}
