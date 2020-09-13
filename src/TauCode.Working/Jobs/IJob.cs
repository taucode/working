using System;
using System.IO;
using TauCode.Working.Schedules;

namespace TauCode.Working.Jobs
{
    public interface IJob : IDisposable
    {
        string Name { get; }
        ISchedule Schedule { get; }
        bool UpdateSchedule(ISchedule schedule);
        JobDelegate Routine { get; set; }
        object Parameter { get; set; }
        IProgressTracker ProgressTracker { get; set; }
        TextWriter Output { get; set; }
        JobInfo GetInfo(int? maxRunCount);
        void OverrideDueTime(DateTimeOffset? dueTime);
        void ForceStart();
    }
}
