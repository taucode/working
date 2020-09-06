using System;

namespace TauCode.Working.Schedules
{
    public interface IScheduledWorker : IWorker
    {
        ISchedule Schedule { get; set; }
        event Action<ScheduleChangedEventArgs> ScheduleChanged;
    }
}
