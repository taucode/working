using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Extensions;
using TauCode.Extensions.Lab;
using TauCode.Infrastructure.Time;
using TauCode.Working.Exceptions;
using TauCode.Working.Schedules;

// todo clean up
namespace TauCode.Working.Jobs.Omicron
{
    internal class OmicronEmployee : IDisposable
    {
        #region Nested

        #region RunContext

        private class RunContext : IDisposable
        {
            private readonly StringWriterWithEncoding _systemWriter;

            private readonly JobRunInfoCollection _runs;

            private readonly JobRunInfoBuilder _runInfoBuilder;
            private readonly CancellationTokenSource _tokenSource;

            private readonly Task _task;
            private Task _endTask;

            internal RunContext(
                JobDelegate routine,
                object parameter,
                IProgressTracker progressTracker,
                TextWriter jobWriter,
                CancellationToken? token,
                JobRunInfoCollection runs,
                JobStartReason startReason,
                DateTimeOffset dueTime,
                bool dueTimeWasOverridden,
                DateTimeOffset startTime)
            {
                _systemWriter = new StringWriterWithEncoding(Encoding.UTF8);
                var writers = new List<TextWriter>
                {
                    _systemWriter,
                };

                if (jobWriter != null)
                {
                    writers.Add(jobWriter);
                }

                var multiTextWriter = new MultiTextWriterLab(Encoding.UTF8, writers);

                if (token == null)
                {
                    _tokenSource = new CancellationTokenSource();
                }
                else
                {
                    _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token.Value);
                }

                _task = routine(parameter, progressTracker, multiTextWriter, _tokenSource.Token);

                _runs = runs;

                _runInfoBuilder = new JobRunInfoBuilder(
                    _runs.Count,
                    startReason,
                    dueTime,
                    dueTimeWasOverridden,
                    startTime,
                    JobRunStatus.Running,
                    _systemWriter);
            }

            internal void Run()
            {
                _endTask = _task.ContinueWith(
                    this.EndTask,
                    CancellationToken.None);
            }

            private void EndTask(Task task)
            {
                var now = TimeProvider.GetCurrent();

                JobRunStatus status;

                switch (task.Status)
                {
                    case TaskStatus.RanToCompletion:
                        status = JobRunStatus.Succeeded;
                        break;

                    case TaskStatus.Canceled:
                        status = JobRunStatus.Canceled;
                        break;

                    case TaskStatus.Faulted:
                        status = JobRunStatus.Faulted;
                        break;

                    default:
                        status = JobRunStatus.Unknown;
                        break;
                }

                _runInfoBuilder.EndTime = now;
                _runInfoBuilder.Status = status;

                var jobRunInfo = _runInfoBuilder.Build();
                _runs.Add(jobRunInfo);
            }

            internal JobRunInfo GetJobRunInfo() => _runInfoBuilder.Build();

            public void Dispose()
            {
                _systemWriter?.Dispose();
                _tokenSource?.Dispose();
            }

            internal bool Wait(TimeSpan timeout)
            {
                try
                {
                    return _endTask.Wait(timeout);
                }
                catch
                {
                    // called in Dispose, should not throw.
                    return false;
                }
            }

            internal bool Wait(int millisecondsTimeout)
            {
                try
                {
                    return _endTask.Wait(millisecondsTimeout);
                }
                catch
                {
                    // called in Dispose, should not throw.
                    return false;
                }
            }

            internal void Cancel()
            {
                _tokenSource.Cancel();
            }
        }

        #endregion

        #region JobRunInfoCollection

        private class JobRunInfoCollection
        {
            private readonly List<JobRunInfo> _list;
            private readonly object _lock;

            internal JobRunInfoCollection()
            {
                _list = new List<JobRunInfo>();
                _lock = new object();
            }

            internal void Add(JobRunInfo jobRunInfo)
            {
                lock (_lock)
                {
                    _list.Add(jobRunInfo);
                }
            }

            internal IReadOnlyList<JobRunInfo> ToList()
            {
                lock (_lock)
                {
                    return _list.ToList();
                }
            }

            internal int Count
            {
                get
                {
                    lock (_lock)
                    {
                        return _list.Count;
                    }
                }
            }
        }

        #endregion

        #endregion

        #region Fields

        private readonly OmicronVice _vice;
        private readonly OmicronJob _job;

        private bool _isEnabled;

        private ISchedule _schedule;
        private DateTimeOffset _scheduleDueTime;
        private DateTimeOffset? _overriddenDueTime;

        private JobDelegate _routine;
        private object _parameter;
        private IProgressTracker _progressTracker;
        private TextWriter _output;

        private readonly JobRunInfoCollection _runs;

        private readonly object _marinaLock;
        private RunContext _runContext;
        private bool _isDisposed;

        #endregion

        #region Constructor

        internal OmicronEmployee(OmicronVice vice, string name)
        {
            this.Name = name;

            _vice = vice;
            _job = new OmicronJob(this);
            _schedule = NeverSchedule.Instance;
            _routine = JobExtensions.IdleJobRoutine;
            _runs = new JobRunInfoCollection();

            _marinaLock = new object();

            this.UpdateScheduleDueTime(); // updated in ctor (todo: check other places)
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
            Action action,
            bool throwIfDisposed,
            bool throwIfNotStopped,
            bool updateScheduleDueTime,
            bool pulseVice)
        {
            lock (_marinaLock)
            {
                if (this.IsDisposed && throwIfDisposed)
                {
                    throw new JobObjectDisposedException(this.Name);
                }

                var isStopped = _runContext == null;

                if (!isStopped && throwIfNotStopped)
                {
                    throw new NotImplementedException();
                }

                action();

                if (updateScheduleDueTime)
                {
                    this.UpdateScheduleDueTime();
                }

                if (pulseVice)
                {
                    _vice.PulseWork();
                }
            }
        }

        private void UpdateScheduleDueTime()
        {
            var now = TimeProvider.GetCurrent();

            lock (_marinaLock)
            {
                _scheduleDueTime = _schedule.GetDueTimeAfter(now.AddTicks(1));
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
            set => this.InvokeWithDataLock(
                action: () => _isEnabled = value,
                throwIfDisposed: true,
                throwIfNotStopped: false,
                updateScheduleDueTime: false,
                pulseVice: true);
        }

        internal ISchedule Schedule
        {
            get => this.GetWithDataLock(() => _schedule);
            set => this.InvokeWithDataLock(
                action: () => _schedule = value,
                throwIfDisposed: true,
                throwIfNotStopped: false,
                updateScheduleDueTime: true,
                pulseVice: true);
        }

        internal JobDelegate Routine
        {
            get => this.GetWithDataLock(() => _routine);
            set => this.InvokeWithDataLock(
                action: () => _routine = value,
                throwIfDisposed: true,
                throwIfNotStopped: true,
                updateScheduleDueTime: false,
                pulseVice: false);
        }

        internal object Parameter
        {
            get => this.GetWithDataLock(() => _parameter);
            set => this.InvokeWithDataLock(
                action: () => _parameter = value,
                throwIfDisposed: true,
                throwIfNotStopped: true,
                updateScheduleDueTime: false,
                pulseVice: false);
        }

        internal IProgressTracker ProgressTracker
        {
            get => this.GetWithDataLock(() => _progressTracker);
            set => this.InvokeWithDataLock(
                action: () => _progressTracker = value,
                throwIfDisposed: true,
                throwIfNotStopped: true,
                updateScheduleDueTime: false,
                pulseVice: false);
        }

        internal TextWriter Output
        {
            get => this.GetWithDataLock(() => _output);
            set => this.InvokeWithDataLock(
                action: () => _output = value,
                throwIfDisposed: true,
                throwIfNotStopped: true,
                updateScheduleDueTime: false,
                pulseVice: false);
        }

        internal JobInfo GetInfo(int? maxRunCount)
        {
            return this.GetWithDataLock(() =>
            {
                var currentRun = _runContext?.GetJobRunInfo();

                return new JobInfo(
                    currentRun,
                    _overriddenDueTime ?? _scheduleDueTime,
                    _overriddenDueTime.HasValue,
                    _runs.Count,
                    _runs.ToList());
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

        internal DueTimeInfoForVice? GetDueTimeInfoForVice()
        {
            if (this.IsDisposed)
            {
                return null;
            }

            lock (_marinaLock)
            {
                var dueTime = _overriddenDueTime ?? _scheduleDueTime;
                var isOverridden = _overriddenDueTime.HasValue;

                var info = new DueTimeInfoForVice(dueTime, isOverridden);
                return info;
            }
        }

        internal bool WakeUp(JobStartReason startReason, CancellationToken? token)
        {
            this.UpdateScheduleDueTime();

            lock (_marinaLock)
            {
                if (_runContext != null)
                {
                    throw new NotImplementedException();
                }

                var now = TimeProvider.GetCurrent();

                _runContext = new RunContext(
                    _routine,
                    _parameter,
                    _progressTracker,
                    _output,
                    token,
                    _runs,
                    startReason,
                    _overriddenDueTime ?? _scheduleDueTime,
                    _overriddenDueTime.HasValue,
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
