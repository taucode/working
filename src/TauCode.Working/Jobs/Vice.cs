using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Extensions.Lab;
using TauCode.Infrastructure.Time;
using TauCode.Working.Exceptions;
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

        //private class EmployeeRecord
        //{
        //    internal EmployeeRecord(Employee employee)
        //    {
        //        this.Employee = employee;
        //        this.DueTimeInfoBuilder = new DueTimeInfoBuilder();
        //        this.Schedule = NeverSchedule.Instance;
        //    }

        //    internal Employee Employee { get; }

        //    internal DueTimeInfoBuilder DueTimeInfoBuilder { get; }

        //    internal ISchedule Schedule { get; set; }
        //}

        #endregion

        #region Fields

        //private readonly Dictionary<string, EmployeeRecord> _employeeRecords;
        private AutoResetEvent _scheduleChangedSignal; // disposed by LoopWorkerBase.Shutdown

        //private readonly object _lock;

        private readonly Dictionary<string, Employee> _employees;
        private readonly object _lock;
        private TimeSpan _vacationTimeout;

        #endregion

        #region Constructor

        internal Vice()
        {

            //_employeeRecords = new Dictionary<string, EmployeeRecord>();
            //_lock = new object();

            _employees = new Dictionary<string, Employee>();
            _lock = new object();
        }

        #endregion

        #region Private

        // todo: cached roster of closest records
        //private EmployeeRecord GetClosestRecord()
        //{
        //    var employeeRecords = this.GetWithControlLock(() => _employeeRecords.Values.ToList());

        //    //if (this.WorkerIsDisposed())
        //    //{
        //    //    return null; // Don't throw exception, just return. Because it's an internal method used in routine loop.
        //    //}

        //    if (employeeRecords.Count == 0)
        //    {
        //        return null; // no candidates.
        //    }

        //    var min = JobExtensions.Never;
        //    EmployeeRecord bestRecord = null;

        //    foreach (var employeeRecord in employeeRecords)
        //    {
        //        var recordDueTime = employeeRecord.DueTimeInfoBuilder.DueTime;

        //        if (recordDueTime < min)
        //        {
        //            bestRecord = employeeRecord;
        //            min = recordDueTime;
        //        }
        //    }

        //    return bestRecord;
        //}

        private void CheckNotDisposed()
        {
            if (this.WorkerIsDisposed())
            {
                throw new JobObjectDisposedException(typeof(IJobManager).FullName);
            }
        }

        // todo: must be called from 'with ctrl lock'
        //private EmployeeRecord TryGetEmployeeRecord(string jobName, bool mustExist)
        //{
        //    lock (_lock)
        //    {
        //        this.CheckNotDisposed();
        //        _employeeRecords.TryGetValue(jobName, out var employeeRecord);

        //        if (employeeRecord == null && mustExist)
        //        {
        //            throw new InvalidJobOperationException($"Job not found: '{jobName}'.");
        //        }

        //        return employeeRecord;
        //    }
        //}

        #endregion

        #region Overridden

        protected override Task<WorkFinishReason> DoWorkAsyncImpl()
        {
            // we cannot be disposed here.

            var now = TimeProvider.GetCurrent();
            var employeesToWakeUp = new List<Employee>();
            var earliest = JobExtensions.Never;

            lock (_lock)
            {
                foreach (var employee in _employees.Values)
                {
                    var dueTime = employee.GetDueTimeForVice();
                    if (!dueTime.HasValue)
                    {
                        continue;
                    }

                    if (now >= dueTime.Value)
                    {
                        // due time has come!
                        employeesToWakeUp.Add(employee);
                    }
                    else
                    {
                        earliest = DateTimeExtensionsLab.Min(earliest, dueTime.Value);
                    }
                }
            }

            foreach (var employee in employeesToWakeUp)
            {
                employee.WakeUp();
            }

            _vacationTimeout = earliest - now;
            _vacationTimeout = DateTimeExtensionsLab.Min(_vacationTimeout, VeryLongVacation);

            return Task.FromResult(WorkFinishReason.WorkIsDone);


            //throw new NotImplementedException();
            //EmployeeRecord employeeRecord;
            //DateTimeOffset lastDueTime;
            //DueTimeInfo lastDueTimeInfo;
            ////bool needToStart;


            //lock (_lock)
            //{
            //    employeeRecord = this.GetClosestRecord();

            //    if (employeeRecord == null)
            //    {
            //        // nothing to work on.
            //        return Task.FromResult(WorkFinishReason.WorkIsDone);
            //    }

            //    lastDueTime = employeeRecord.DueTimeInfoBuilder.DueTime;
            //    var now = TimeProvider.GetCurrent();
            //    if (lastDueTime <= now)
            //    {
            //        // gotta start this job

            //        // job will start with current due time info
            //        lastDueTimeInfo = employeeRecord.DueTimeInfoBuilder.Build();

            //        // update due time info for the next job start.
            //        employeeRecord.DueTimeInfoBuilder.UpdateBySchedule(employeeRecord.Schedule, now);
            //    }
            //    else
            //    {
            //        // too early, won't start the job.
            //        return Task.FromResult(WorkFinishReason.WorkIsDone);
            //    }
            //}

            //// start the job
            //var jobStartResult = employeeRecord.Employee.StartJob(
            //    JobStartReason.DueTime,
            //    lastDueTimeInfo);

            //string message;
            //switch (jobStartResult)
            //{
            //    case JobStartResult.AlreadyStartedByDueTime:
            //        message = $"'{employeeRecord.Employee.Name}' wast already started before by due time.";
            //        this.GetLogger().Information(message, nameof(DoWorkAsyncImpl));
            //        break;

            //    case JobStartResult.AlreadyStartedByForce:
            //        throw new NotImplementedException();
            //        break;

            //    case JobStartResult.Started:
            //        message = $"'{employeeRecord.Employee.Name}' wast successfully started by due time '{lastDueTimeInfo.DueTime}'.";
            //        this.GetLogger().Information(message, nameof(DoWorkAsyncImpl));
            //        break;

            //    case JobStartResult.AlreadyDisposed:
            //        throw new NotImplementedException();
            //        break;

            //    default:
            //        throw new ArgumentOutOfRangeException();
            //}


            ////var now = this.GetCurrentTime();

            ////var dueTime = employeeRecord.DueTimeInfoBuilder.DueTime;



            ////if (dueTime <= now)
            ////{
            ////    // gotta run this job
            ////    var dueTimeInfo = employeeRecord.DueTimeInfoBuilder.Build();
            ////    var jobStartResult = employeeRecord.Employee.StartJob(
            ////        StartReason.DueTime,
            ////        dueTimeInfo);

            ////    // job now has new due time
            ////    //employeeRecord.DueTimeInfoBuilder.UpdateBySchedule(employeeRecord.Schedule, now);

            ////    // let's analyze job start result
            ////    string message;
            ////    switch (jobStartResult)
            ////    {
            ////        case JobStartResult.AlreadyStartedByDueTime:
            ////            throw new NotImplementedException();
            ////            break;

            ////        case JobStartResult.AlreadyStartedByForce:
            ////            throw new NotImplementedException();
            ////            break;

            ////        case JobStartResult.Started:
            ////            message = $"'{employeeRecord.Employee.Name}' wast started by due time.";
            ////            break;

            ////        case JobStartResult.AlreadyDisposed:
            ////            throw new NotImplementedException();
            ////            break;

            ////        default:
            ////            throw new ArgumentOutOfRangeException();
            ////    }
            ////}

            //return Task.FromResult(WorkFinishReason.WorkIsDone);
        }

        protected override Task<VacationFinishReason> TakeVacationAsyncImpl()
        {
            _vacationTimeout = DateTimeExtensionsLab.Max(_vacationTimeout, TimeQuantum);



            //var calculatedVacationTimeout = this.GetWithControlLock(() =>
            //{
            //    // we cannot be 'Disposed' here.
            //    TimeSpan? vacationTimeout;

            //    var employeeRecord = this.GetClosestRecord();
            //    if (employeeRecord == null)
            //    {
            //        vacationTimeout = VeryLongVacation; // no candidates, let's party for ~1mo.
            //    }
            //    else
            //    {
            //        var dueTime = employeeRecord.DueTimeInfoBuilder.DueTime;
            //        var now = TimeProvider.GetCurrent();
            //        if (dueTime <= now)
            //        {
            //            // oh, we've got a due time elapsed, terminate vacation immediately!
            //            vacationTimeout = null;
            //        }
            //        else
            //        {
            //            // got some time to have fun before the due time
            //            vacationTimeout = dueTime - now;
            //            if (vacationTimeout > VeryLongVacation)
            //            {
            //                vacationTimeout = VeryLongVacation;
            //            }
            //            else
            //            {
            //                // let's be prepped a bit before due time
            //                vacationTimeout -= Leeway;
            //                if (vacationTimeout <= TimeSpan.Zero)
            //                {
            //                    // must be positive
            //                    vacationTimeout = TimeQuantum;
            //                }
            //            }
            //        }
            //    }

            //    return vacationTimeout;
            //});

            //if (!calculatedVacationTimeout.HasValue)
            //{
            //    // got work to do
            //    return Task.FromResult(VacationFinishReason.VacationTimeElapsed);
            //}

            var signalIndex = this.WaitForControlSignalWithExtraSignals(_vacationTimeout);

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
            //IList<Employee> employeesToFire = _employeeRecords
            //    .Values
            //    .Select(x => x.Employee)
            //    .ToList();

            //_employeeRecords.Clear();

            //foreach (var employee in employeesToFire)
            //{
            //    employee.Dispose(); // must not throw, 'Dispose' is graceful (I hope)
            //}

            IList<Employee> employees;

            lock (_lock)
            {
                employees = _employees.Values.ToList();
                _employees.Clear();
            }

            foreach (var employee in employees)
            {
                employee.Dispose();
            }

            base.DisposeImpl();
        }

        #endregion

        #region Internal - called by IJobManager (can throw)

        internal IJob CreateJob(string jobName)
        {
            return this.GetWithControlLock(() =>
            {
                this.CheckNotDisposed();

                lock (_lock)
                {
                    var existing = _employees.GetValueOrDefault(jobName);

                    //var existing = this.TryGetEmployeeRecord(jobName, false);


                    if (existing != null)
                    {
                        throw new InvalidJobOperationException($"Job '{jobName}' already exists.");
                    }

                    var employee = new Employee(this)
                    {
                        Name = jobName,
                    };

                    _employees.Add(employee.Name, employee);

                    return employee.GetJob();
                }



                //var employeeRecord = new EmployeeRecord(employee);
                //_employeeRecords.Add(employee.Name, employeeRecord);

                //var now = TimeProvider.GetCurrent();

                //employeeRecord.DueTimeInfoBuilder.UpdateBySchedule(employeeRecord.Schedule, now);
                //_scheduleChangedSignal.Set();

                
            });
        }

        internal IReadOnlyList<string> GetJobNames()
        {
            return this.GetWithControlLock(() =>
            {
                this.CheckNotDisposed();

                lock (_lock)
                {
                    return _employees
                        .Values
                        .Select(x => x.Name)
                        .ToList();
                }
            });
        }

        #endregion

        #region Internal - called by Employee

        internal void FireMe(string jobName)
        {
            this.InvokeWithControlLock(() => // don't let 'Dispose' myself while I am firing an employee, so '_scheduleChangedSignal' is 'alive'.
            {
                // we are not calling 'CheckNotDisposed' here because 'Fire' is only called by IJob.Dispose => Employee.GetFired => Vice.Fire
                if (this.WorkerIsDisposed())
                {
                    // Dear pal (Employee instance), I am disposed myself. And this means you will be fired (disposed) in a while, too.
                    // If you want to get fired (and not yet), just feel free to go. I won't throw any exceptions.
                    return;
                }

                throw new NotImplementedException();
                //_employeeRecords.Remove(jobName);
                //_scheduleChangedSignal.Set();
            });
        }

        #endregion

        #region Internal






        //internal IJob GetJob(string jobName) => this.TryGetEmployeeRecord(jobName, true).Employee.GetJob();

        #endregion

        //internal DateTimeOffset GetCurrentTime() => TimeProvider.GetCurrent();

        //internal DueTimeInfo GetDueTimeInfo(string jobName)
        //{
        //    lock (_lock)
        //    {
        //        var employeeRecord = this.TryGetEmployeeRecord(jobName, true);
        //        return employeeRecord.DueTimeInfoBuilder.Build();
        //    }
        //}

        internal void OverrideDueTime(string jobName, DateTimeOffset? dueTime)
        {
            // todo check due time not in past (+ut)
            throw new NotImplementedException();
            //lock (_lock)
            //{
            //    var employeeRecord = this.TryGetEmployeeRecord(jobName, true);

            //    if (dueTime.HasValue)
            //    {
            //        employeeRecord.DueTimeInfoBuilder.UpdateManually(dueTime.Value);
            //    }
            //    else
            //    {
            //        //var now = this.GetCurrentTime();
            //        var now = TimeProvider.GetCurrent();
            //        employeeRecord.DueTimeInfoBuilder.UpdateBySchedule(employeeRecord.Schedule, now);
            //    }
            //}

            //_scheduleChangedSignal.Set();
        }

        //internal ISchedule GetSchedule(string jobName)
        //{
        //    lock (_lock)
        //    {
        //        var employeeRecord = this.TryGetEmployeeRecord(jobName, true);
        //        return employeeRecord.Schedule;
        //    }
        //}

        //internal void UpdateSchedule(string jobName, ISchedule schedule)
        //{
        //    lock (_lock)
        //    {
        //        var employeeRecord = this.TryGetEmployeeRecord(jobName, true);

        //        if (employeeRecord.DueTimeInfoBuilder.Type == DueTimeType.Overridden)
        //        {
        //            throw new NotImplementedException();
        //        }

        //        var now = TimeProvider.GetCurrent();

        //        employeeRecord.Schedule = schedule;
        //        employeeRecord.DueTimeInfoBuilder.UpdateBySchedule(employeeRecord.Schedule, now);
        //    }

        //    _scheduleChangedSignal.Set();
        //}

        internal void DebugPulse()
        {
            _scheduleChangedSignal.Set();
        }


        // todo: called by Employee.OnScheduleChanged
        internal void OnScheduleChanged()
        {
            // todo: call this when CreateJob, RemoveJob, etc.

            throw new NotImplementedException();
        }
    }
}
