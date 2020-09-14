﻿using System;
using System.IO;
using TauCode.Working.Schedules;

// todo clean
namespace TauCode.Working.Jobs
{
    internal class Job : IJob
    {
        #region Fields

        private readonly Employee _employee;

        #endregion

        #region Constructor

        internal Job(Employee employee)
        {
            _employee = employee;
        }

        #endregion

        #region IJob Members (explicit)

        string IJob.Name => _employee.Name;

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

        #endregion

        #region IDisposable Members (explicit)

        void IDisposable.Dispose() => _employee.GetFired();

        #endregion
    }
}
