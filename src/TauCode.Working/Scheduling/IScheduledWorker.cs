using System;

namespace TauCode.Working.Scheduling
{
    public interface IScheduledWorker : IWorker
    {
        ISchedule Schedule { get; set; }
        event Action<ScheduleChangedEventArgs> ScheduleChanged;
    }
}
