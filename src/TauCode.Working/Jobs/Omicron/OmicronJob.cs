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

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string Name => _employee.Name;

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

        public void OverrideDueTime(DateTimeOffset? dueTime)
        {
            throw new NotImplementedException();
        }

        public void ForceStart()
        {
            throw new NotImplementedException();
        }
    }
}
