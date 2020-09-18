using System;
using System.IO;
using System.Threading;
using TauCode.Working.Jobs.Instruments;
using TauCode.Working.Schedules;

namespace TauCode.Working.Jobs
{
    internal class Employee : IDisposable
    {
        #region Fields

        private readonly Vice _vice;
        private readonly Job _job;
        private readonly Runner _runner;

        #endregion

        #region Constructor

        internal Employee(Vice vice, string name)
        {
            this.Name = name;

            _vice = vice;
            _job = new Job(this);
            _runner = new Runner(this.Name);
        }

        #endregion

        #region IJob Implementation

        /// <summary>
        /// Returns <see cref="IJob"/> instance itself.
        /// </summary>
        /// <returns><see cref="IJob"/> instance itself</returns>
        internal IJob GetJob() => _job;

        internal string Name { get; }

        internal bool IsEnabled
        {
            get => _runner.IsEnabled;
            set => _runner.IsEnabled = value;
        }

        internal ISchedule Schedule
        {
            get => _runner.DueTimeHolder.Schedule;
            set
            {
                _runner.DueTimeHolder.Schedule = value;
                _vice.PulseWork();
            }
        }

        internal JobDelegate Routine
        {
            get => _runner.JobPropertiesHolder.Routine;
            set => _runner.JobPropertiesHolder.Routine = value;
        }

        internal object Parameter
        {
            get => _runner.JobPropertiesHolder.Parameter;
            set => _runner.JobPropertiesHolder.Parameter = value;
        }

        internal IProgressTracker ProgressTracker
        {
            get => _runner.JobPropertiesHolder.ProgressTracker;
            set => _runner.JobPropertiesHolder.ProgressTracker = value;
        }

        internal TextWriter Output
        {
            get => _runner.JobPropertiesHolder.Output;
            set => _runner.JobPropertiesHolder.Output = value;
        }

        internal JobInfo GetInfo(int? maxRunCount) => _runner.GetInfo(maxRunCount);

        internal void OverrideDueTime(DateTimeOffset? dueTime) => this._runner.DueTimeHolder.OverriddenDueTime = dueTime;

        internal void ForceStart() => this.WakeUp(JobStartReason.Force, null);

        internal bool Cancel() => _runner.Cancel();

        internal bool Wait(int millisecondsTimeout) => throw new NotImplementedException();

        internal bool Wait(TimeSpan timeout) => throw new NotImplementedException();

        internal bool IsDisposed => _runner.IsDisposed;

        #endregion

        #region Interface for Vice

        internal DueTimeInfo? GetDueTimeInfoForVice(bool future) => _runner.GetDueTimeInfoForVice(future);

        internal bool WakeUp(JobStartReason startReason, CancellationToken? token) => _runner.WakeUp(startReason, token);

        #endregion

        #region IDisposable Members

        public void Dispose() => _runner.Dispose();

        #endregion
    }
}
