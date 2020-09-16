using System;
using System.IO;
using TauCode.Working.Schedules;

namespace TauCode.Working.Jobs.Omicron
{
    internal class OmicronJob : IJob
    {
        private readonly OmicronEmployee _employee;

        internal OmicronJob(OmicronEmployee employee)
        {
            _employee = employee;
        }

        public void Dispose() => _employee.Dispose();

        public string Name => _employee.Name;

        public bool IsEnabled
        {
            get => _employee.IsEnabled;
            set => _employee.IsEnabled = value;
        }

        public ISchedule Schedule
        {
            get => _employee.Schedule;
            set => _employee.Schedule = value ?? throw new ArgumentNullException(nameof(IJob.Schedule));
        }

        public JobDelegate Routine
        {
            get => _employee.Routine;
            set => _employee.Routine = value ?? throw new ArgumentNullException(nameof(IJob.Routine));
        }

        public object Parameter
        {
            get => _employee.Parameter;
            set => throw new NotImplementedException();
        }

        public IProgressTracker ProgressTracker
        {
            get => _employee.ProgressTracker;
            set => throw new NotImplementedException();
        }

        public TextWriter Output
        {
            get => _employee.Output;
            set => _employee.Output = value;
        }

        public JobInfo GetInfo(int? maxRunCount) => _employee.GetInfo(maxRunCount);

        public void OverrideDueTime(DateTimeOffset? dueTime) => _employee.OverrideDueTime(dueTime);

        public void ForceStart() => _employee.ForceStart();

        public bool Cancel() => _employee.Cancel();

        public bool IsDisposed => _employee.IsDisposed;
    }
}
