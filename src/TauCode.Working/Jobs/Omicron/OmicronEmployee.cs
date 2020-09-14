using System;
using System.IO;
using TauCode.Labor;
using TauCode.Working.Schedules;

namespace TauCode.Working.Jobs.Omicron
{
    internal class OmicronEmployee : ProlBase
    {
        private readonly OmicronVice _vice;
        private readonly OmicronJob _job;
        private ISchedule _schedule;
        private JobDelegate _routine;
        private object _parameter;
        private IProgressTracker _progressTracker;
        private TextWriter _output;

        internal OmicronEmployee(OmicronVice vice)
        {
            _vice = vice;
            _job = new OmicronJob(this);
            _schedule = NeverSchedule.Instance;
            _routine = JobExtensions.IdleJobRoutine;
        }

        internal IJob GetJob() => _job;

        internal ISchedule Schedule => _schedule;

        internal JobDelegate Routine => _routine;

        internal object Parameter => _parameter;

        internal IProgressTracker ProgressTracker => _progressTracker;

        internal TextWriter Output => _output;
    }

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
            set => throw new NotImplementedException();
        }

        public JobDelegate Routine
        {
            get => _employee.Routine;
            set => throw new NotImplementedException();
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
            set => throw new NotImplementedException();
        }

        public JobInfo GetInfo(int? maxRunCount)
        {
            throw new NotImplementedException();
        }

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
