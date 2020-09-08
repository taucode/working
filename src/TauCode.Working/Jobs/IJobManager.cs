using System;
using System.Threading.Tasks;

namespace TauCode.Working.Jobs
{
    public interface IJobManager : IDisposable
    {
        void Start();

        void RegisterJob(string jobName, Func<Task> jobTaskCreator, ISchedule jobSchedule);

        void ChangeJobSchedule(string jobName, ISchedule newJobSchedule);

        void EnableJob(string jobName, bool enable);

        void RemoveJob(string jobName);
    }
}
