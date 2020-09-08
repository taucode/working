using System;

namespace TauCode.Working.Jobs.Schedules
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
