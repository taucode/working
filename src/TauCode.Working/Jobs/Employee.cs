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
        #region Constants

        private const long MillisecondsToDispose = 25;

        #endregion

        #region Fields

        private readonly Vice _vice;
        private readonly Job _job;

        private bool _isEnabled;

        private readonly JobRunsHolder _runsHolder;
        private readonly DueTimeHolder _dueTimeHolder;
        private RunContext _runContext;

        private JobDelegate _routine;
        private object _parameter;
        private IProgressTracker _progressTracker;
        private TextWriter _output;

        private readonly object _marinaLock;
        private bool _isDisposed;

        #endregion

        #region Constructor

        internal Employee(Vice vice, string name)
        {
            this.Name = name;

            _vice = vice;
            _job = new Job(this);
            _routine = JobExtensions.IdleJobRoutine;
            _runsHolder = new JobRunsHolder();

            _marinaLock = new object();

            _dueTimeHolder = new DueTimeHolder(NeverSchedule.Instance);
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

        private void InvokeWithDataLock(Action action)
        {
            lock (_marinaLock)
            {
                if (this.IsDisposed)
                {
                    throw new JobObjectDisposedException(this.Name);
                }

                action();
            }
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
            get => this.GetWithDataLock(() => _isEnabled);
            set => this.InvokeWithDataLock(action: () => _isEnabled = value);
        }

        internal ISchedule Schedule
        {
            get => _dueTimeHolder.Schedule;
            set => this.InvokeWithDataLock(
                action: () =>
                {
                    _dueTimeHolder.Schedule = value;
                    _vice.PulseWork();
                });
        }

        internal JobDelegate Routine
        {
            get => this.GetWithDataLock(() => _routine);
            set => this.InvokeWithDataLock(action: () => _routine = value);
        }

        internal object Parameter
        {
            get => this.GetWithDataLock(() => _parameter);
            set => this.InvokeWithDataLock(action: () => _parameter = value);
        }

        internal IProgressTracker ProgressTracker
        {
            get => this.GetWithDataLock(() => _progressTracker);
            set => this.InvokeWithDataLock(action: () => _progressTracker = value);
        }

        internal TextWriter Output
        {
            get => this.GetWithDataLock(() => _output);
            set => this.InvokeWithDataLock(action: () => _output = value);
        }

        internal JobInfo GetInfo(int? maxRunCount)
        {
            return this.GetWithDataLock(() =>
            {
                var tuple = _runsHolder.Get();
                var currentRun = tuple.Item1;
                var runs = tuple.Item2;

                var dueTimeInfo = _dueTimeHolder.GetDueTimeInfo();

                return new JobInfo(
                    currentRun,
                    dueTimeInfo.GetEffectiveDueTime(),
                    dueTimeInfo.IsDueTimeOverridden(),
                    _runsHolder.Count,
                    runs);
            });
        }

        internal void OverrideDueTime(DateTimeOffset? dueTime)
        {
            _dueTimeHolder.OverriddenDueTime = dueTime;
            _vice.PulseWork();
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
                // todo: ugly. reorganize.
                DueTimeInfo? result = null;

                if (this.IsDisposed)
                {
                    // return null
                }
                else
                {
                    if (future)
                    {
                        _dueTimeHolder.UpdateScheduleDueTime();
                    }

                    result = _dueTimeHolder.GetDueTimeInfo();
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
                    _dueTimeHolder.UpdateScheduleDueTime(); // I am having PTO right now, but you come visit me another time, Vice!
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
                    _dueTimeHolder,
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
