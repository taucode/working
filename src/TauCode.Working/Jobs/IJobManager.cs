using System;
using System.Collections.Generic;

namespace TauCode.Working.Jobs
{
    public interface IJobManager : IDisposable
    {
        void Start();

        //void Register(
        //    string jobName,
        //    Func<object, TextWriter, CancellationToken, Task> jobTaskCreator,
        //    ISchedule jobSchedule,
        //    object parameter);

        //void SetParameter(string jobName, object parameter);

        //object GetParameter(string jobName);

        //void AddProgressTracker(string jobName, string progressTrackerName, IProgressTracker progressTracker);

        //IReadOnlyList<string> GetProgressTrackerNames(string jobName);

        //void RemoveProgressTracker(string jobName, string progressTrackerName);

        IJob Create(string jobName);

        IReadOnlyList<string> GetJobNames();

        void Set(string jobName, IJob job);

        IJob Get(string jobName);

        //void SetSchedule(string jobName, ISchedule schedule);

        //ISchedule GetSchedule(string jobName);

        void ManualChangeDueTime(string jobName, DateTime? dueTime);

        IReadOnlyList<DateTime> GetSchedulePart(string jobName, int length);

        void ForceStart(string jobName);

        //void RedirectOutput(string jobName, TextWriter output);

        void Cancel(string jobName);

        void Enable(string jobName, bool enable);

        JobInfo GetInfo(string jobName, int? maxRunCount);

        void Remove(string jobName);

        JobRunWaitResult Wait(string jobName, TimeSpan timeout);

        event EventHandler<JobChangedEventArgs> JobChanged;
    }
}
