using System;
using System.IO;
using TauCode.Working.Jobs.Schedules;

namespace TauCode.Working.Jobs
{
    internal class Job : IJob
    {
        #region Fields

        private readonly Employee _doer;

        private ISchedule _schedule;
        private JobDelegate _routine;
        private object _parameter;
        private IProgressTracker _progressTracker;
        private TextWriter _output;

        private readonly object _lock;

        #endregion

        #region Constructor

        internal Job(Employee doer)
        {
            _doer = doer;

            _schedule = new NeverSchedule();
            _routine = JobExtensions.IdleJobRoutine;

            _lock = new object();
        }

        #endregion

        #region IJob Members (explicit)

        ISchedule IJob.Schedule
        {
            get
            {
                lock (_lock)
                {
                    return _schedule;
                }
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                lock (_lock)
                {
                    _schedule = value;
                    throw new NotImplementedException();
                    //_host.UpdateDueTimeInfo()
                }
            }
        }

        JobDelegate IJob.Routine
        {
            get => _doer.GetWithControlLock(() => _routine);
            set => throw new NotImplementedException();
        }

        object IJob.Parameter
        {
            get => _doer.GetWithControlLock(() => _parameter);
            set => throw new NotImplementedException();
        }

        IProgressTracker IJob.ProgressTracker
        {
            get => _doer.GetWithControlLock(() => _progressTracker);
            set => throw new NotImplementedException();
        }

        TextWriter IJob.Output
        {
            get => _doer.GetWithControlLock(() => _output);
            set => throw new NotImplementedException();
        }

        #endregion
    }
}
