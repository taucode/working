using System;

namespace TauCode.Working.Scheduling
{
    public class ScheduleChangedEventArgs : EventArgs
    {
        public ScheduleChangedEventArgs(IScheduledWorker scheduledWorker)
        {
            this.ScheduledWorker = scheduledWorker ?? throw new ArgumentNullException(nameof(scheduledWorker));
        }

        public IScheduledWorker ScheduledWorker { get; }
    }
}
