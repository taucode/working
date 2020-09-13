using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Infrastructure.Time;
using TauCode.Working.Exceptions;
using TauCode.Working.Schedules;
using TauCode.Working.Workers;

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

        private readonly Dictionary<string, EmployeeRecord> _employeeRecords;
        private AutoResetEvent _scheduleChangedSignal; // disposed by LoopWorkerBase.Shutdown

        private readonly object _lock;

        #endregion

        #region Constructor

        internal Vice()
        {
            _employeeRecords = new Dictionary<string, EmployeeRecord>();
            _lock = new object();
        }

        #endregion

        #region Private

        // todo: cached roster of closest records
        private EmployeeRecord GetClosestRecord()
        {
            if (this.WorkerIsDisposed())
            {
                return null; // Don't throw exception, just return. Because it's an internal method used in routine loop.
            }

            if (_employeeRecords.Count == 0)
            {
                return null; // no candidates.
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

        private void CheckNotDisposed()
        {
            if (this.WorkerIsDisposed())
            {
                throw new JobObjectDisposedException(typeof(IJobManager).FullName);
            }
        }

        private EmployeeRecord TryGetEmployeeRecord(string jobName, bool mustExist)
        {
            lock (_lock)
            {
                this.CheckNotDisposed();
                _employeeRecords.TryGetValue(jobName, out var employeeRecord);

                if (employeeRecord == null && mustExist)
                {
                    throw new InvalidJobOperationException($"Job not found: '{jobName}'.");
                }

                return employeeRecord;
            }
        }

        #endregion

        #region Overridden

        protected override Task<WorkFinishReason> DoWorkAsyncImpl()
        {
            EmployeeRecord employeeRecord;

            lock (_lock)
            {
                employeeRecord = this.GetClosestRecord();

                if (employeeRecord == null)
                {
                    // nothing to work on.
                    return Task.FromResult(WorkFinishReason.WorkIsDone);
                }
            }

            //var now = this.GetCurrentTime();
            var now = TimeProvider.GetCurrent();
            var dueTime = employeeRecord.DueTimeInfoBuilder.DueTime;

            if (dueTime <= now)
            {
                // gotta run this job
                var dueTimeInfo = employeeRecord.DueTimeInfoBuilder.Build();
                var jobStartResult = employeeRecord.Employee.StartJob(
                    StartReason.DueTime,
                    dueTimeInfo);

                // job now has new due time
                employeeRecord.DueTimeInfoBuilder.UpdateBySchedule(employeeRecord.Schedule, now);

                // let's analyze job start result
                switch (jobStartResult)
                {
                    case JobStartResult.AlreadyStartedByDueTime:
                        throw new NotImplementedException();
                        break;

                    case JobStartResult.AlreadyStartedByForce:
                        throw new NotImplementedException();
                        break;

                    case JobStartResult.Started:
                        throw new NotImplementedException();
                        break;

                    case JobStartResult.AlreadyDisposed:
                        throw new NotImplementedException();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
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
                    //var now = this.GetCurrentTime();
                    var now = TimeProvider.GetCurrent();
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
            _scheduleChangedSignal = new AutoResetEvent(false);
            return new[] { _scheduleChangedSignal };
        }

        protected override void DisposeImpl()
        {
            IList<Employee> employeesToFire;

            lock (_lock)
            {
                employeesToFire = _employeeRecords
                    .Values
                    .Select(x => x.Employee)
                    .ToList();

                _employeeRecords.Clear();
            }

            foreach (var employee in employeesToFire)
            {
                employee.Dispose(); // must not throw, 'Dispose' is graceful (I hope)
            }

            base.DisposeImpl();
        }

        #endregion

        #region Internal


        internal IReadOnlyList<string> GetJobNames()
        {
            lock (_lock)
            {
                this.CheckNotDisposed();
                return _employeeRecords.Keys.ToList();
            }
        }

        internal IJob CreateJob(string jobName)
        {
            lock (_lock)
            {
                this.CheckNotDisposed();

                var existing = this.TryGetEmployeeRecord(jobName, false);
                if (existing != null)
                {
                    throw new InvalidJobOperationException($"Job '{jobName}' already exists.");
                }

                var employee = new Employee(this)
                {
                    Name = jobName,
                };

                var employeeRecord = new EmployeeRecord(employee);
                _employeeRecords.Add(employee.Name, employeeRecord);

                var now = TimeProvider.GetCurrent();

                employeeRecord.DueTimeInfoBuilder.UpdateBySchedule(employeeRecord.Schedule, now);
                _scheduleChangedSignal.Set();

                return employee.GetJob();
            }
        }

        internal IJob GetJob(string jobName) => this.TryGetEmployeeRecord(jobName, true).Employee.GetJob();

        #endregion

        //internal DateTimeOffset GetCurrentTime() => TimeProvider.GetCurrent();

        internal DueTimeInfo GetDueTimeInfo(string jobName)
        {
            lock (_lock)
            {
                var employeeRecord = this.TryGetEmployeeRecord(jobName, true);
                return employeeRecord.DueTimeInfoBuilder.Build();
            }
        }

        internal void OverrideDueTime(string jobName, DateTimeOffset? dueTime)
        {
            // todo check due time not in past (+ut)
            lock (_lock)
            {
                var employeeRecord = this.TryGetEmployeeRecord(jobName, true);

                if (dueTime.HasValue)
                {
                    employeeRecord.DueTimeInfoBuilder.UpdateManually(dueTime.Value);
                }
                else
                {
                    //var now = this.GetCurrentTime();
                    var now = TimeProvider.GetCurrent();
                    employeeRecord.DueTimeInfoBuilder.UpdateBySchedule(employeeRecord.Schedule, now);
                }
            }

            _scheduleChangedSignal.Set();
        }

        internal ISchedule GetSchedule(string jobName)
        {
            lock (_lock)
            {
                var employeeRecord = this.TryGetEmployeeRecord(jobName, true);
                return employeeRecord.Schedule;
            }
        }

        internal void UpdateSchedule(string jobName, ISchedule schedule)
        {
            lock (_lock)
            {
                var employeeRecord = this.TryGetEmployeeRecord(jobName, true);

                if (employeeRecord.DueTimeInfoBuilder.Type == DueTimeType.Overridden)
                {
                    throw new NotImplementedException();
                }

                var now = TimeProvider.GetCurrent();

                employeeRecord.Schedule = schedule;
                employeeRecord.DueTimeInfoBuilder.UpdateBySchedule(employeeRecord.Schedule, now);
            }

            _scheduleChangedSignal.Set();
        }

        internal void DebugPulse()
        {
            _scheduleChangedSignal.Set();
        }

        internal void Fire(string jobName)
        {
            this.InvokeWithControlLock(() => // don't let 'Dispose' myself while I am firing an employee, so '_scheduleChangedSignal' is 'alive'.
            {
                if (this.WorkerIsDisposed())
                {
                    // Pal (Employee instance), I am disposed myself. And this means you will be fired (disposed) in a while, too.
                    // If you want to get fired (and not yet), just feel free to go. I won't throw any exceptions.
                    return;
                }

                lock (_lock)
                {
                    _employeeRecords.Remove(jobName);
                }

                _scheduleChangedSignal.Set();
            });
        }
    }
}
