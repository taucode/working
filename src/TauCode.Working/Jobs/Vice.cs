using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Infrastructure.Time;
using TauCode.Working.Jobs.Schedules;

namespace TauCode.Working.Jobs
{
    // todo clean up
    internal class Vice : LoopWorkerBase
    {
        #region Constants

        private const int ScheduleChangedSignalIndex = 1;

        private static readonly TimeSpan VeryLongVacation = TimeSpan.FromMilliseconds(int.MaxValue);

        /// <summary>
        /// Minimal waitable time period is 1 millisecond.
        /// </summary>
        private static readonly TimeSpan TimeQuantum = TimeSpan.FromMilliseconds(1);
        
        /// <summary>
        /// If we have a due-time candidate, we've gotta break vacation a little bit before his actual due time.
        /// </summary>
        private static readonly TimeSpan Leeway = TimeSpan.FromMilliseconds(10);

        #endregion

        #region Nested

        private class EmployeeRecord
        {
            internal EmployeeRecord(Employee employee)
            {
                this.Employee = employee;
                this.DueTimeInfoBuilder = new DueTimeInfoBuilder();
                this.Schedule = NeverSchedule.Instance;
            }

            internal Employee Employee { get; }

            internal DueTimeInfoBuilder DueTimeInfoBuilder { get; }

            internal ISchedule Schedule { get; set; }
        }

        #endregion

        #region Fields

        //private readonly ITimeProvider _timeProvider;
        private readonly Dictionary<string, EmployeeRecord> _employeeRecords;
        private AutoResetEvent _scheduleChangedEvent; // disposed by LoopWorkerBase.Shutdown

        private readonly object _lock;

        #endregion

        #region Constructor

        internal Vice()
        {
            //_timeProvider = timeProvider;
            _employeeRecords = new Dictionary<string, EmployeeRecord>();
            _lock = new object();
        }

        #endregion

        #region Private
        
        private EmployeeRecord GetClosestRecord()
        {
            lock (_lock) // todo: excessive locking?
            {
                if (_employeeRecords.Count == 0)
                {
                    return null;
                }

                var min = JobExtensions.Never;
                EmployeeRecord bestRecord = null;

                foreach (var employeeRecords in _employeeRecords.Values)
                {
                    var recordDueTime = employeeRecords.DueTimeInfoBuilder.DueTime;

                    if (recordDueTime < min)
                    {
                        bestRecord = employeeRecords;
                        min = recordDueTime;
                    }
                }

                return bestRecord;
            }
        }
        #endregion

        #region Overridden

        protected override Task<WorkFinishReason> DoWorkAsyncImpl()
        {
            lock (_lock)
            {
                var employeeRecord = this.GetClosestRecord();

                if (employeeRecord == null)
                {
                    // nothing to work on.
                    return Task.FromResult(WorkFinishReason.WorkIsDone);
                }
                else
                {
                    var now = this.GetCurrentTime();
                    var dueTime = employeeRecord.DueTimeInfoBuilder.DueTime;

                    if (dueTime <= now)
                    {
                        // gotta run this job
                        employeeRecord.DueTimeInfoBuilder.UpdateBySchedule(employeeRecord.Schedule, now);

                        // todo: check & ut job is not running already.
                        Task.Run(() => employeeRecord.Employee.StartJob(StartReason.DueTime, employeeRecord.DueTimeInfoBuilder.Build()));
                    }
                }
            }

            return Task.FromResult(WorkFinishReason.WorkIsDone);
        }

        protected override Task<VacationFinishReason> TakeVacationAsyncImpl()
        {
            TimeSpan vacationTimeout;

            lock (_lock)
            {
                var employeeRecord = this.GetClosestRecord();
                if (employeeRecord == null)
                {
                    vacationTimeout = VeryLongVacation; // no candidates, let's party for ~1mo.
                }
                else
                {
                    var dueTime = employeeRecord.DueTimeInfoBuilder.DueTime;
                    var now = this.GetCurrentTime();
                    if (dueTime <= now)
                    {
                        // oh, we've got a due time elapsed, terminate vacation immediately!
                        return Task.FromResult(VacationFinishReason.VacationTimeElapsed);
                    }
                    else
                    {
                        // got some time to have fun before the due time
                        vacationTimeout = dueTime - now;
                        if (vacationTimeout > VeryLongVacation)
                        {
                            vacationTimeout = VeryLongVacation;
                        }
                        else
                        {
                            // let's be prepped a bit before due time
                            vacationTimeout -= Leeway;
                            if (vacationTimeout <= TimeSpan.Zero)
                            {
                                // must be positive
                                vacationTimeout = TimeQuantum;
                            }
                        }
                    }
                }
            }

            var signalIndex = this.WaitForControlSignalWithExtraSignals(vacationTimeout);

            switch (signalIndex)
            {
                case ControlSignalIndex:
                    return Task.FromResult(VacationFinishReason.GotControlSignal);

                case ScheduleChangedSignalIndex:
                    return Task.FromResult(VacationFinishReason.NewWorkArrived);

                case WaitHandle.WaitTimeout:
                    // our vacation is over.
                    return Task.FromResult(VacationFinishReason.VacationTimeElapsed);

                default:
                    throw this.CreateInternalErrorException(); // should never happen
            }
        }

        protected override IList<AutoResetEvent> CreateExtraSignals()
        {
            _scheduleChangedEvent = new AutoResetEvent(false);
            return new[] { _scheduleChangedEvent };
        }

        #endregion

        #region Internal


        internal IReadOnlyList<string> GetJobNames()
        {
            lock (_lock)
            {
                return _employeeRecords.Keys.ToList();
            }
        }

        internal IJob CreateJob(string jobName)
        {
            lock (_lock)
            {
                // todo: check name doesn't exist
                var employee = new Employee(this)
                {
                    Name = jobName,
                };

                var employeeRecord = new EmployeeRecord(employee);
                _employeeRecords.Add(employee.Name, employeeRecord);

                var now = this.GetCurrentTime();

                employeeRecord.DueTimeInfoBuilder.UpdateBySchedule(employeeRecord.Schedule, now);
                _scheduleChangedEvent.Set();

                return employee.GetJob();
            }
        }

        internal IJob GetJob(string jobName)
        {
            lock (_lock)
            {
                return _employeeRecords[jobName].Employee.GetJob(); // todo check exists, here & anywhere.
            }
        }

      

        #endregion

        internal DateTime GetCurrentTime() => TimeProvider.GetCurrent();

        internal DueTimeInfo GetDueTimeInfo(string jobName)
        {
            lock (_lock)
            {
                var employeeRecord = _employeeRecords[jobName]; // todo check
                return employeeRecord.DueTimeInfoBuilder.Build();
            }
        }

        internal void OverrideDueTime(string jobName, DateTime? dueTime)
        {
            // todo check due time not in past
            lock (_lock)
            {
                var employeeRecord = _employeeRecords[jobName]; // todo check

                if (dueTime.HasValue)
                {
                    employeeRecord.DueTimeInfoBuilder.UpdateManually(dueTime.Value);
                }
                else
                {
                    var now = this.GetCurrentTime();
                    employeeRecord.DueTimeInfoBuilder.UpdateBySchedule(employeeRecord.Schedule, now);
                }
            }

            _scheduleChangedEvent.Set();
        }

        internal ISchedule GetSchedule(string name)
        {
            lock (_lock)
            {
                var employeeRecord = _employeeRecords[name];
                return employeeRecord.Schedule;
            }
        }

        internal void UpdateSchedule(string name, ISchedule schedule)
        {
            lock (_lock)
            {
                var employeeRecord = _employeeRecords[name]; // todo check

                if (employeeRecord.DueTimeInfoBuilder.Type == DueTimeType.Overridden)
                {
                    throw new NotImplementedException();
                }

                var now = this.GetCurrentTime();

                employeeRecord.Schedule = schedule;
                employeeRecord.DueTimeInfoBuilder.UpdateBySchedule(employeeRecord.Schedule, now);
            }

            _scheduleChangedEvent.Set();
        }

        internal void DebugPulse()
        {
            _scheduleChangedEvent.Set();
        }
    }
}
