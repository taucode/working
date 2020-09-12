using System;
using TauCode.Working.Jobs;

namespace TauCode.Working.Schedules
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
