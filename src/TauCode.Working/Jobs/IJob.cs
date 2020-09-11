using System;
using System.IO;

namespace TauCode.Working.Jobs
{
    public interface IJob
    {
        ISchedule Schedule { get; }
        bool UpdateSchedule(ISchedule schedule);
        JobDelegate Routine { get; set; }
        object Parameter { get; set; }
        IProgressTracker ProgressTracker { get; set; }
        TextWriter Output { get; set; }
        JobInfo GetInfo(int? maxRunCount);
        void OverrideDueTime(DateTime? dueTime);
        void ForceStart();
    }
}
