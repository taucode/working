using System;
using System.IO;

namespace TauCode.Working.Jobs
{
    internal class Job : IJob
    {
        #region Fields

        private readonly Employee _doer;

        #endregion

        #region Constructor

        internal Job(Employee doer)
        {
            _doer = doer;
        }

        #endregion

        #region IJob Members (explicit)

        ISchedule IJob.Schedule => _doer.GetSchedule();

        public bool UpdateSchedule(ISchedule schedule)
        {
            if (schedule == null)
            {
                throw new ArgumentNullException(nameof(schedule));
            }

            return _doer.UpdateSchedule(schedule);
        }

        JobDelegate IJob.Routine
        {
            get => _doer.GetRoutine();
            set => _doer.SetRoutine(value);
        }

        object IJob.Parameter
        {
            get => _doer.GetParameter();
            set => _doer.SetParameter(value);
        }

        IProgressTracker IJob.ProgressTracker
        {
            get => _doer.GetProgressTracker();
            set => _doer.SetProgressTracker(value);
        }

        TextWriter IJob.Output
        {
            get => _doer.GetOutput();
            set => _doer.SetOutput(value);
        }

        JobInfo IJob.GetInfo(int? maxRunCount) => _doer.GetJobInfo(maxRunCount);

        void IJob.OverrideDueTime(DateTime? dueTime)
        {
            _doer.OverrideDueTime(dueTime);
        }

        void IJob.ForceStart()
        {
            _doer.ForceStart();
        }

        #endregion
    }
}
