using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TauCode.Working.Jobs
{
    public interface IJobManager : IDisposable
    {
        void Start();

        void Register(
            string jobName,
            Func<object, TextWriter, CancellationToken, Task> jobTaskCreator,
            ISchedule jobSchedule,
            object parameter);

        void SetParameter(string jobName, object parameter);

        object GetParameter(string jobName);

        void SetSchedule(string jobName, ISchedule schedule);

        ISchedule GetSchedule(string jobName);

        void ManualChangeDueTime(string jobName, DateTime? dueTime);

        IList<DateTime> GetSchedulePart(string jobName, int length);

        void ForceStart(string jobName);

        void RedirectOutput(string jobName, TextWriter output);

        void Cancel(string jobName);

        void Enable(string jobName, bool enable);

        JobInfo GetInfo(string jobName, int? maxRunCount);

        void Remove(string jobName);

        event EventHandler<JobChangedEventArgs> JobChanged;
    }
}
