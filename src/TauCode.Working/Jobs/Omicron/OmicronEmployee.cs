using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Extensions;
using TauCode.Extensions.Lab;
using TauCode.Infrastructure.Time;
using TauCode.Labor;
using TauCode.Labor.Exceptions;
using TauCode.Working.Exceptions;
using TauCode.Working.Schedules;

namespace TauCode.Working.Jobs.Omicron
{
    internal class OmicronEmployee : ProlBase
    {
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

        private MultiTextWriterLab _currentWriter;
        private JobRunInfoBuilder _currentInfoBuilder;
        private CancellationTokenSource _currentTokenSource;
        private Task _currentTask;
        private Task _currentEndTask;

        private readonly List<JobRunInfo> _runs;
        private int _runIndex;

        private readonly object _dataLock;

        private readonly object _marinaLock;


        #endregion

        #region Constructor

        internal OmicronEmployee(OmicronVice vice)
        {
            _vice = vice;
            _job = new OmicronJob(this);
            _schedule = NeverSchedule.Instance;
            _routine = JobExtensions.IdleJobRoutine;
            _runs = new List<JobRunInfo>();

            _dataLock = new object();

            _marinaLock = new object();

            // todo: update in GetInfo, also (?)
            this.UpdateScheduleDueTime(); // updated in ctor
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

                if (this.State != ProlState.Stopped && throwIfNotStopped)
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

        #endregion

        #region IJob Implementation

        internal bool IsEnabled
        {
            get => this.GetWithDataLock(() => _isEnabled);
            //{
            //    lock (_dataLock)
            //    {
            //        return _isEnabled;
            //    }
            //}
            set => this.InvokeWithDataLock(
                action: () => _isEnabled = value,
                throwIfDisposed: true,
                throwIfNotStopped: false,
                updateScheduleDueTime: false,
                pulseVice: true);

            //{
            //    lock (_dataLock)
            //    {
            //        // todo: universal method 'CheckIsDisposed'
            //        if (this.IsDisposed)
            //        {
            //            throw new JobObjectDisposedException(this.Name);
            //        }

            //        _isEnabled = value;
            //        _vice.PulseWork();
            //    }
            //}
        }

        internal ISchedule Schedule
        {
            get => this.GetWithDataLock(() => _schedule);
            //{
            //    lock (_dataLock)
            //    {
            //        return _schedule;
            //    }
            //}
            set => this.InvokeWithDataLock(
                action: () => _schedule = value,
                throwIfDisposed: true,
                throwIfNotStopped: false,
                updateScheduleDueTime: true,
                pulseVice: true);
            //{
            //    lock (_dataLock)
            //    {
            //        if (this.IsDisposed)
            //        {
            //            throw new NotImplementedException();
            //        }

            //        _schedule = value;
            //        this.UpdateScheduleDueTime(); // updated in Schedule.set
            //        _vice.PulseWork();
            //    }
            //}
        }

        internal JobDelegate Routine
        {
            get => this.GetWithDataLock(() => _routine);
            //{
            //    lock (_dataLock)
            //    {
            //        return _routine;
            //    }
            //}
            set => this.InvokeWithDataLock(
                action: () => _routine = value,
                throwIfDisposed: true,
                throwIfNotStopped: true,
                updateScheduleDueTime: false,
                pulseVice: false);
            //{
            //    lock (_dataLock)
            //    {
            //        if (this.State == ProlState.Running)
            //        {
            //            throw new NotImplementedException();
            //        }

            //        _routine = value;
            //    }
            //}
        }

        internal object Parameter
        {
            get => this.GetWithDataLock(() => _parameter);
            //{
            //    lock (_dataLock)
            //    {
            //        return _parameter;
            //    }
            //}
            set => this.InvokeWithDataLock(
                action: () => _parameter = value,
                throwIfDisposed: true,
                throwIfNotStopped: true,
                updateScheduleDueTime: false,
                pulseVice: false);
            //{
            //    lock (_dataLock)
            //    {
            //        if (this.State == ProlState.Running)
            //        {
            //            throw new NotImplementedException();
            //        }

            //        _parameter = value;
            //    }
            //}
        }

        internal IProgressTracker ProgressTracker
        {
            get => this.GetWithDataLock(() => _progressTracker);
            //{
            //    lock (_dataLock)
            //    {
            //        return _progressTracker;
            //    }
            //}
            set => this.InvokeWithDataLock(
                action: () => _progressTracker = value,
                throwIfDisposed: true,
                throwIfNotStopped: true,
                updateScheduleDueTime: false,
                pulseVice: false);
            //{
            //    lock (_dataLock)
            //    {
            //        if (this.State == ProlState.Running)
            //        {
            //            throw new NotImplementedException();
            //        }

            //        _progressTracker = value;
            //    }
            //}
        }

        internal TextWriter Output
        {
            get => this.GetWithDataLock(() => _output);
            //{
            //    lock (_dataLock)
            //    {
            //        return _output;
            //    }
            //}
            set => this.InvokeWithDataLock(
                action: () => _output  = value,
                throwIfDisposed: true,
                throwIfNotStopped: true,
                updateScheduleDueTime: false,
                pulseVice: false);
            //{
            //    lock (_dataLock)
            //    {
            //        if (this.State == ProlState.Running)
            //        {
            //            throw new NotImplementedException();
            //        }

            //        _output = value;
            //    }
            //}
        }

        #endregion


        private void UpdateScheduleDueTime()
        {
            var now = TimeProvider.GetCurrent();

            lock (_dataLock)
            {
                _scheduleDueTime = _schedule.GetDueTimeAfter(now.AddTicks(1));
            }
        }

        private DateTimeOffset GetEffectiveDueTime()
        {
            lock (_dataLock)
            {
                return _overriddenDueTime ?? _scheduleDueTime;
            }
        }

        internal IJob GetJob() => _job;


        internal DateTimeOffset? GetDueTimeForVice()
        {
            if (this.IsDisposed)
            {
                return null;
            }

            return this.GetEffectiveDueTime();
        }

        private void EndJob(Task task) => this.Stop(false);

        protected override void OnStopped()
        {
            lock (_dataLock)
            {
                if (_currentEndTask != null)
                {
                    this.FinalizeJobRun();
                }
            }
        }

        private void FinalizeJobRun()
        {
            var now = TimeProvider.GetCurrent();

            _currentInfoBuilder.EndTime = now;
            var jobOutputWriter = (StringWriter)_currentWriter.InnerWriters[0];

            switch (_currentTask.Status)
            {
                case TaskStatus.Faulted:
                    var ex = _currentTask.Exception.InnerException;
                    if (ex is JobFailedToStartException)
                    {
                        _currentInfoBuilder.Status = JobRunStatus.FailedToStart;
                    }
                    else
                    {
                        _currentInfoBuilder.Status = JobRunStatus.Failed;
                    }

                    _currentInfoBuilder.Exception = ex;

                    break;

                case TaskStatus.RanToCompletion:
                    _currentInfoBuilder.Status = JobRunStatus.Succeeded;
                    break;

                case TaskStatus.Canceled:
                    _currentInfoBuilder.Status = JobRunStatus.Canceled;
                    break;

                default:
                    _currentInfoBuilder.Status =
                        JobRunStatus.Unknown; // actually, very strange, but we cannot throw here.
                    break;
            }

            var runInfo = _currentInfoBuilder.Build();
            _runs.Add(runInfo);

            jobOutputWriter.Dispose();

            _currentWriter.Dispose();
            _currentWriter = null;

            _currentInfoBuilder = null;

            _currentTokenSource.Dispose();
            _currentTokenSource = null;

            _currentTask = null;
            _currentEndTask = null;

            this.UpdateScheduleDueTime(); // updated in FinalizeJobRun
        }

        internal WakeUpResult WakeUp(CancellationToken token)
        {
            lock (_dataLock)
            {
                try
                {
                    this.Start(); // todo0000 BAAAD!!!
                    _currentTask = this.InitJobRunContext(true, token);

                    _overriddenDueTime = null;
                    this.UpdateScheduleDueTime(); // updated in WakeUp

                    _currentEndTask = _currentTask.ContinueWith(this.EndJob);

                    switch (_currentInfoBuilder.StartReason)
                    {
                        case JobStartReason.ScheduleDueTime:
                            return WakeUpResult.StartedBySchedule;

                        case JobStartReason.OverriddenDueTime:
                            return WakeUpResult.StartedByOverriddenDueTime;

                        case JobStartReason.Force2:
                            return WakeUpResult.UnexpectedlyStartedByForce;

                        default:
                            return WakeUpResult.Unknown;
                    }
                }
                catch (InappropriateProlStateException)
                {
                    return WakeUpResult.AlreadyRunning;
                }
                catch (ObjectDisposedException)
                {
                    return WakeUpResult.AlreadyDisposed;
                }
            }
        }

        private Task InitJobRunContext(bool byDueTime, CancellationToken? token)
        {
            var jobWriter = new StringWriterWithEncoding(Encoding.UTF8);
            var writers = new List<TextWriter>
            {
                jobWriter,
            };

            if (_output != null)
            {
                writers.Add(_output);
            }

            var now = TimeProvider.GetCurrent();

            _currentWriter = new MultiTextWriterLab(Encoding.UTF8, writers);

            if (token.HasValue)
            {
                _currentTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token.Value);
            }
            else
            {
                _currentTokenSource = new CancellationTokenSource();
            }

            JobStartReason reason;

            if (byDueTime)
            {
                reason = _overriddenDueTime.HasValue
                    ? JobStartReason.OverriddenDueTime
                    : JobStartReason.ScheduleDueTime;
            }
            else
            {
                reason = JobStartReason.Force2;
            }

            _currentInfoBuilder = new JobRunInfoBuilder(
                _runIndex,
                reason,
                this.GetEffectiveDueTime(),
                _overriddenDueTime.HasValue,
                now,
                JobRunStatus.Unknown,
                jobWriter);

            _runIndex++;

            Task task;

            try
            {
                task = _routine(_parameter, _progressTracker, _currentWriter, _currentTokenSource.Token);
            }
            catch (Exception ex)
            {
                // todo: wrong. _routine might throw exception intentionally.
                // todo: deal with completed tasks?
                var jobEx = new JobFailedToStartException(ex);
                task = Task.FromException(jobEx);
            }

            return task;
        }

        internal JobInfo GetInfo(int? maxRunCount)
        {
            lock (_dataLock)
            {
                var currentRun = _currentInfoBuilder?.Build();

                return new JobInfo(
                    currentRun,
                    this.GetEffectiveDueTime(),
                    _overriddenDueTime.HasValue,
                    _runs.Count,
                    _runs);
            }
        }

        internal bool Cancel()
        {
            lock (_dataLock)
            {
                if (this.State == ProlState.Stopped)
                {
                    return false;
                }

                _currentTokenSource?.Cancel();
                this.Stop();
                return true;
            }
        }

        internal void ForceStart()
        {
            lock (_dataLock)
            {

                this.Start(); // todo baad!
                _currentTask = this.InitJobRunContext(false, null);

                _overriddenDueTime = null;
                this.UpdateScheduleDueTime(); // updated in ForceStart

                _currentEndTask = _currentTask.ContinueWith(this.EndJob);
            }
        }
    }

    // todo separate file
    internal enum WakeUpResult
    {
        Unknown = 0,

        AlreadyDisposed = 1,
        AlreadyRunning = 2,

        StartedBySchedule = 3,
        StartedByOverriddenDueTime = 4,
        UnexpectedlyStartedByForce = 5,
    }
}
