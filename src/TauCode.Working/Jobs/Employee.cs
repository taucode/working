using System;
using System.IO;
using System.Threading;
using TauCode.Infrastructure.Time;
using TauCode.Working.Exceptions;
using TauCode.Working.Jobs.Instruments;
using TauCode.Working.Schedules;

// todo clean up
namespace TauCode.Working.Jobs
{
    internal class Employee : IDisposable
    {
        #region Nested



        #region ScheduleHolder

        

        #endregion

        #endregion

        #region Constants

        private const long MillisecondsToDispose = 25;

        #endregion

        #region Fields

        private readonly Vice _vice;
        private readonly Job _job;

        private bool _isEnabled;

        //private ISchedule _schedule;
        //private DateTimeOffset _scheduleDueTime;

        private readonly JobRunsHolder _runsHolder;
        private readonly ScheduleHolder _scheduleHolder;

        private JobDelegate _routine;
        private object _parameter;
        private IProgressTracker _progressTracker;
        private TextWriter _output;

        private readonly object _marinaLock;
        private RunContext _runContext;
        private bool _isDisposed;

        #endregion

        #region Constructor

        internal Employee(Vice vice, string name)
        {
            this.Name = name;

            _vice = vice;
            _job = new Job(this);

            //_schedule = NeverSchedule.Instance;
            _routine = JobExtensions.IdleJobRoutine;
            _runsHolder = new JobRunsHolder();

            _marinaLock = new object();

            _scheduleHolder = new ScheduleHolder(_vice, NeverSchedule.Instance);

            //this.UpdateScheduleDueTime(); // updated in ctor (todo: check other places)
        }

        #endregion

        #region Private

        private T GetWithDataLock<T>(Func<T> getter)
        {
            lock (_marinaLock)
            {
                var result = getter();
                return result;
            }
        }

        private void InvokeWithDataLock(
            Action action//,
            //bool throwIfDisposed,
            //bool throwIfNotStopped,
            //bool updateScheduleDueTime,
            //bool pulseVice
            )
        {
            lock (_marinaLock)
            {
                if (this.IsDisposed /*&& throwIfDisposed*/)
                {
                    throw new JobObjectDisposedException(this.Name);
                }

                //var isStopped = _runContext == null;

                //if (!isStopped && throwIfNotStopped)
                //{
                //    throw new NotImplementedException();
                //}

                action();

                //if (updateScheduleDueTime)
                //{
                //    this.UpdateScheduleDueTime();
                //}

                //if (pulseVice)
                //{
                //    _vice.PulseWork();
                //}
            }
        }

        //private void UpdateScheduleDueTime()
        //{
        //    var now = TimeProvider.GetCurrent();

        //    lock (_marinaLock)
        //    {
        //        _scheduleDueTime = _schedule.GetDueTimeAfter(now.AddTicks(1));
        //    }
        //}

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
            get => this.GetWithDataLock(() => _isEnabled);
            set => this.InvokeWithDataLock(
                action: () => _isEnabled = value//,
                //throwIfDisposed: true,
                //throwIfNotStopped: false,
                //updateScheduleDueTime: false,
                //pulseVice: true
                );
        }

        internal ISchedule Schedule
        {
            get => _scheduleHolder.Schedule; //this.GetWithDataLock(() => _schedule);
            set => this.InvokeWithDataLock(
                action: () =>
                {
                    _scheduleHolder.Schedule = value;
                    _vice.PulseWork();
                }

                //,
                //throwIfDisposed: true,
                //throwIfNotStopped: false,
                //updateScheduleDueTime: true,
                //pulseVice: true
                );
        }

        internal JobDelegate Routine
        {
            get => this.GetWithDataLock(() => _routine);
            set => this.InvokeWithDataLock(
                action: () => _routine = value//,
                //throwIfDisposed: true,
                //throwIfNotStopped: true,
                //updateScheduleDueTime: false,
                //pulseVice: false
                );
        }

        internal object Parameter
        {
            get => this.GetWithDataLock(() => _parameter);
            set => this.InvokeWithDataLock(
                action: () => _parameter = value//,
                //throwIfDisposed: true,
                //throwIfNotStopped: true,
                //updateScheduleDueTime: false,
                //pulseVice: false
                );
        }

        internal IProgressTracker ProgressTracker
        {
            get => this.GetWithDataLock(() => _progressTracker);
            set => this.InvokeWithDataLock(
                action: () => _progressTracker = value//,
                //throwIfDisposed: true,
                //throwIfNotStopped: true,
                //updateScheduleDueTime: false,
                //pulseVice: false
                );
        }

        internal TextWriter Output
        {
            get => this.GetWithDataLock(() => _output);
            set => this.InvokeWithDataLock(
                action: () => _output = value//,
                //throwIfDisposed: true,
                //throwIfNotStopped: true,
                //updateScheduleDueTime: false,
                //pulseVice: false
                );
        }

        internal JobInfo GetInfo(int? maxRunCount)
        {
            return this.GetWithDataLock(() =>
            {
                var tuple = _runsHolder.Get();
                var currentRun = tuple.Item1;
                var runs = tuple.Item2;

                var dueTimeInfo = _scheduleHolder.GetDueTimeInfo();

                return new JobInfo(
                    currentRun,
                    //_scheduleHolder.GetEffectiveDueTime(),
                    dueTimeInfo.GetEffectiveDueTime(),
                    dueTimeInfo.IsDueTimeOverridden(),
                    //_overriddenDueTime ?? _scheduleDueTime,
                    //_overriddenDueTime.HasValue,
                    _runsHolder.Count,
                    runs);
            });
        }

        internal void OverrideDueTime(DateTimeOffset? dueTime)
        {
            throw new NotImplementedException();
        }

        internal void ForceStart()
        {
            this.WakeUp(JobStartReason.Force2, null);
        }

        internal bool Cancel()
        {
            lock (_marinaLock)
            {
                if (_runContext == null)
                {
                    return false;
                }
                else
                {
                    _runContext.Cancel();
                    _runContext = null;
                    return true;
                }
            }
        }

        internal bool Wait(int millisecondsTimeout)
        {
            if (this.IsDisposed)
            {
                throw new NotImplementedException(); // cannot wait on disposed obj.
            }

            RunContext runContext;
            lock (_marinaLock)
            {
                runContext = _runContext;
            }

            var gotSignal = runContext?.Wait(millisecondsTimeout) ?? true;
            return gotSignal;
        }

        internal bool Wait(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        internal bool IsDisposed
        {
            get
            {
                lock (_marinaLock)
                {
                    return _isDisposed;
                }
            }
        }

        #endregion

        #region Interface for Vice

        internal DueTimeInfo? GetDueTimeInfoForVice(bool future)
        {
            return this.GetWithDataLock(() =>
            {
                DueTimeInfo? result = null;

                if (this.IsDisposed)
                {
                    // return null
                }
                else
                {
                    if (future)
                    {
                        _scheduleHolder.UpdateScheduleDueTime();
                    }

                    result = _scheduleHolder.GetDueTimeInfo();
                }

                return result;
            });
        }

        internal bool WakeUp(JobStartReason startReason, CancellationToken? token)
        {
            lock (_marinaLock)
            {
                if (_runContext != null)
                {
                    // already running, but you come visit me another time, Vice!
                    return false;
                }

                if (!this.IsEnabled)
                {
                    _scheduleHolder.UpdateScheduleDueTime(); // I am having PTO right now, but you come visit me another time, Vice!
                    return false;
                }

                var now = TimeProvider.GetCurrent();

                _runContext = new RunContext(
                    _routine,
                    _parameter,
                    _progressTracker,
                    _output,
                    token,
                    _runsHolder,
                    _scheduleHolder,
                    startReason,
                    now);

                _runContext.Run();

                return true;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            RunContext runContext;

            lock (_marinaLock)
            {
                if (_isDisposed)
                {
                    return; // won't dispose twice.
                }

                _isDisposed = true;

                runContext = _runContext;
                _runContext = null;
            }

            if (runContext != null)
            {
                try
                {
                    runContext.Cancel();
                    runContext.Dispose();
                }
                catch
                {
                    // Dispose should not throw.
                }
            }
        }

        #endregion
    }
}
