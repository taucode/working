using System;

namespace TauCode.Working.Schedules
{
    public class CronSchedule : ISchedule
    {
        public string Description { get; set; }
        public DateTimeOffset GetDueTimeAfter(DateTimeOffset after)
        {
            throw new NotImplementedException();
        }
    }
}
