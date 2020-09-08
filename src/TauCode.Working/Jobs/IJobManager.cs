using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TauCode.Working.Jobs
{
    public interface IJobManager : IDisposable
    {
        void Start();

        void RegisterJob(
            string jobName,
            Func<TextWriter, CancellationToken, Task<bool>> jobTaskCreator,
            ISchedule jobSchedule);

        void ChangeJobSchedule(string jobName, ISchedule newJobSchedule);

        void ChangeJobDueTime(string jobName, DateTime dueTime);

        void ResetJobDueTime(string jobName);

        void ForceStartJob(string jobName);

        void CancelRunningJob(string jobName);

        void EnableJob(string jobName, bool enable);

        JobInfo GetJobInfo(string jobName, bool includeLog);

        void RemoveJob(string jobName);
    }
}
