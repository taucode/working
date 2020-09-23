using System;
using System.IO;
using TauCode.Working.Jobs;
using TauCode.Working.Schedules;

// todo clean
namespace TauCode.Working.ZetaOld.Jobs
{
    internal class ZetaJob : IJob
    {
        #region Fields

        private readonly ZetaEmployee _employee;

        #endregion

        #region Constructor

        internal ZetaJob(ZetaEmployee employee)
        {
            _employee = employee;
        }

        #endregion

        #region IJob Members (explicit)

        string IJob.Name => _employee.Name;
        public bool IsEnabled
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        ISchedule IJob.Schedule
        {
            get => _employee.GetSchedule();
            set => _employee.UpdateSchedule(value ?? throw new ArgumentNullException(nameof(IJob.Schedule)));
        }

        //public void UpdateSchedule(ISchedule schedule)
        //{
        //    if (schedule == null)
        //    {
        //        throw new ArgumentNullException(nameof(schedule));
        //    }

        //    _employee.UpdateSchedule(schedule);
        //}

        JobDelegate IJob.Routine
        {
            get => _employee.GetRoutine();
            set => _employee.SetRoutine(value);
        }

        object IJob.Parameter
        {
            get => _employee.GetParameter();
            set => _employee.SetParameter(value);
        }

        IProgressTracker IJob.ProgressTracker
        {
            get => _employee.GetProgressTracker();
            set => _employee.SetProgressTracker(value);
        }

        TextWriter IJob.Output
        {
            get => _employee.GetOutput();
            set => _employee.SetOutput(value);
        }

        JobInfo IJob.GetInfo(int? maxRunCount) => _employee.GetJobInfo(maxRunCount);

        void IJob.OverrideDueTime(DateTimeOffset? dueTime)
        {
            _employee.OverrideDueTime(dueTime);
        }

        void IJob.ForceStart()
        {
            throw new NotImplementedException();
            //_employee.ForceStart();
        }

        public bool Cancel()
        {
            throw new NotImplementedException();
        }

        public JobRunStatus? Wait(int millisecondsTimeout)
        {
            throw new NotImplementedException();
        }

        public JobRunStatus? Wait(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public bool IsDisposed => throw new NotImplementedException();

        #endregion

        #region IDisposable Members (explicit)

        void IDisposable.Dispose() => _employee.GetFired();

        #endregion
    }
}
