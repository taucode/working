using System;

namespace TauCode.Working.Scheduling.Schedules
{
    public class CronSchedule : ISchedule
    {
        public string Description { get; set; }
        public DateTime GetDueTimeAfter(DateTime after)
        {
            throw new NotImplementedException();
        }
    }
}
