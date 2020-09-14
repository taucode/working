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

        private readonly object _lock;

        internal OmicronEmployee(OmicronVice vice)
        {
            _vice = vice;
            _job = new OmicronJob(this);
            _schedule = NeverSchedule.Instance;
            _routine = JobExtensions.IdleJobRoutine;

            _lock = new object();
        }

        internal IJob GetJob() => _job;

        internal ISchedule Schedule => _schedule;

        internal JobDelegate Routine
        {
            get
            {
                lock (_lock)
                {
                    return _routine;
                }
            }
            set
            {
                lock (_lock)
                {
                    if (this.State == ProlState.Running)
                    {
                        throw new NotImplementedException();
                    }

                    _routine = value;
                }
            }
        }

        internal object Parameter
        {
            get
            {
                lock (_lock)
                {
                    return _parameter;
                }
            }
            set
            {
                lock (_lock)
                {
                    if (this.State == ProlState.Running)
                    {
                        throw new NotImplementedException();
                    }

                    _parameter = value;
                }
            }
        }

        internal IProgressTracker ProgressTracker
        {
            get
            {
                lock (_lock)
                {
                    return _progressTracker;
                }
            }
            set
            {
                lock (_lock)
                {
                    if (this.State == ProlState.Running)
                    {
                        throw new NotImplementedException();
                    }

                    _progressTracker = value;
                }
            }
        }

        internal TextWriter Output
        {
            get
            {
                lock (_lock)
                {
                    return _output;
                }
            }
            set
            {
                lock (_lock)
                {
                    if (this.State == ProlState.Running)
                    {
                        throw new NotImplementedException();
                    }

                    _output = value;
                }
            }
        }
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
            set => _employee.Output = value;
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
