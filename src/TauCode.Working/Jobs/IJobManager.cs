using System;
using System.Collections.Generic;

namespace TauCode.Working.Jobs
{
    public interface IJobManager : IDisposable
    {
        void Start();

        IJob Create(string jobName);

        IReadOnlyList<string> GetJobNames();

        IJob Get(string jobName);

        IReadOnlyList<DateTime> GetSchedulePart(string jobName, int length);

        void ForceStart(string jobName);

        void Cancel(string jobName);

        void Enable(string jobName, bool enable);

        //JobInfo GetInfo(string jobName, int? maxRunCount);

        void Remove(string jobName);

        JobRunWaitResult Wait(string jobName, TimeSpan timeout);

        event EventHandler<JobChangedEventArgs> JobChanged;
    }
}
