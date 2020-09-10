using System;
using System.IO;
using TauCode.Working.Jobs.Schedules;

namespace TauCode.Working.Jobs
{
    internal class Job : IJob
    {
        private readonly JobWorker _host;

        private ISchedule _schedule;
        private JobDelegate _routine;
        private object _parameter;
        private IProgressTracker _progressTracker;
        private TextWriter _output;

        internal Job(JobWorker host)
        {
            _host = host;

            _schedule = new NeverSchedule();
            _routine = JobExtensions.IdleJobRoutine;
        }

        #region IJob Members (explicit)

        ISchedule IJob.Schedule
        {
            get => _host.RequestWithControlLock(() => _schedule);
            set => throw new NotImplementedException();
        }

        JobDelegate IJob.Routine
        {
            get => _host.RequestWithControlLock(() => _routine);
            set => throw new NotImplementedException();
        }

        object IJob.Parameter
        {
            get => _host.RequestWithControlLock(() => _parameter);
            set => throw new NotImplementedException();
        }

        IProgressTracker IJob.ProgressTracker
        {
            get => _host.RequestWithControlLock(() => _progressTracker);
            set => throw new NotImplementedException();
        }

        TextWriter IJob.Output
        {
            get => _host.RequestWithControlLock(() => _output);
            set => throw new NotImplementedException();
        }

        #endregion
    }
}
