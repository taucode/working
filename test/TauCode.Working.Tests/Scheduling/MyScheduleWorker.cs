using TauCode.Working.Scheduling;

namespace TauCode.Working.Tests.Scheduling
{
    public class MyScheduleWorker : ScheduledWorkerBase
    {
        public MyScheduleWorker(ISchedule schedule)
            : base(schedule)
        {
        }
    }
}
