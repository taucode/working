using System;
using System.IO;
using TauCode.Working.Schedules;

namespace TauCode.Working.Jobs
{
    public interface IJob : IDisposable
    {
        string Name { get; }
        bool IsEnabled { get; set; }
        ISchedule Schedule { get; set; }
        JobDelegate Routine { get; set; }
        object Parameter { get; set; }
        IProgressTracker ProgressTracker { get; set; }
        TextWriter Output { get; set; }
        JobInfo GetInfo(int? maxRunCount);
        void OverrideDueTime(DateTimeOffset? dueTime);
        void ForceStart();
        bool Cancel(); // todo: void, not bool
        bool Wait(int millisecondsTimeout);
        bool Wait(TimeSpan timeout);
        bool IsDisposed { get; }
    }
}
