using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Extensions;
using TauCode.Extensions.Lab;
using TauCode.Infrastructure.Time;
using TauCode.Working.Exceptions;
using TauCode.Working.Schedules;

namespace TauCode.Working.Jobs.Omicron
{
    internal class OmicronEmployee : IDisposable //: ProlBase
    {
        #region Nested

        private class RunContext
        {
            private readonly MultiTextWriterLab _multiTextWriter;
            private readonly StringWriterWithEncoding _systemWriter;
            private readonly Task _task;
            private readonly Action<JobRunInfoBuilder> _callback;

            private readonly JobRunInfoBuilder _runInfoBuilder;
            private readonly CancellationTokenSource _tokenSource;
            //private Task _currentTask;
            //private Task _currentEndTask;

            internal RunContext(
                JobDelegate routine,
                object parameter,
                IProgressTracker progressTracker,
                TextWriter jobWriter,
                CancellationToken? token,
                Action<JobRunInfoBuilder> callback)
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

                _multiTextWriter = new MultiTextWriterLab(Encoding.UTF8, writers);

                if (token == null)
                {
                    _tokenSource = new CancellationTokenSource();
                }
                else
                {
                    _tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token.Value);
                }

                _task = routine(parameter, progressTracker, _multiTextWriter, _tokenSource.Token);

                _callback = callback;
            }

            internal void Run()
            {
                _task.ContinueWith(
                    this.EndTask,
                    _tokenSource.Token,
                    _tokenSource.Token);
            }

            private Task EndTask(Task task, object state)
            {
                throw new NotImplementedException();
            }
        }

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


        //private MultiTextWriterLab _currentWriter;
        //private JobRunInfoBuilder _currentInfoBuilder;
        //private CancellationTokenSource _currentTokenSource;
        //private Task _currentTask;
        //private Task _currentEndTask;

        private readonly List<JobRunInfo> _runs;
        private int _runIndex;

        //private readonly object _dataLock;

        private readonly object _marinaLock;
        private readonly object _wakeUpLock;
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
            _runs = new List<JobRunInfo>();

            //_dataLock = new object();

            _marinaLock = new object();
            _wakeUpLock = new object();

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

                var isStopped = _runContext == null;

                if (!isStopped /*this.State != ProlState.Stopped*/ && throwIfNotStopped)
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

        //private DateTimeOffset GetEffectiveDueTime() =>
        //    this.GetWithDataLock(() => _overriddenDueTime ?? _scheduleDueTime);
        //{
        //    lock (_dataLock)
        //    {
        //        return _overriddenDueTime ?? _scheduleDueTime;
        //    }
        //}

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
                action: () => _output = value,
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

        internal JobInfo GetInfo(int? maxRunCount)
        {
            throw new NotImplementedException();

            //return this.GetWithDataLock(() =>
            //{
            //    var currentRun = _currentInfoBuilder?.Build();

            //    return new JobInfo(
            //        currentRun,
            //        this.GetEffectiveDueTime(),
            //        _overriddenDueTime.HasValue,
            //        _runs.Count,
            //        _runs);
            //});
        }

        internal void OverrideDueTime(DateTimeOffset? dueTime)
        {
            throw new NotImplementedException();
        }

        internal void ForceStart()
        {
            throw new NotImplementedException();
            //lock (_dataLock)
            //{

            //    this.Start(); // todo baad!
            //    _currentTask = this.InitJobRunContext(false, null);

            //    _overriddenDueTime = null;
            //    this.UpdateSch-eduleDueTime(); // updated in ForceStart

            //    _currentEndTask = _currentTask.ContinueWith(this.EndJob);
            //}
        }

        internal bool Cancel()
        {
            throw new NotImplementedException();

            //if (this.State != ProlState.Running)
            //{
            //    return false;
            //}

            //lock (_dataLock)
            //{
            //    if (this.State == ProlState.Stopped)
            //    {
            //        return false;
            //    }

            //    _currentTokenSource?.Cancel();
            //    //this.Stop();
            //    return true;
            //}
        }

        #endregion

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

        private void EndJob(Task task) // => this.Stop(false);
        {
            throw new NotImplementedException();
        }

        //protected override void OnStarting()
        //{
        //    throw new NotImplementedException();
        //}

        //protected override void OnStopped()
        //{
        //    throw new NotImplementedException();

        //    //lock (_dataLock)
        //    //{
        //    //    if (_currentEndTask != null)
        //    //    {
        //    //        this.FinalizeJobRun();
        //    //    }
        //    //}
        //}

        private void FinalizeJobRun()
        {
            throw new NotImplementedException();

            //var now = TimeProvider.GetCurrent();

            //_currentInfoBuilder.EndTime = now;
            //var jobOutputWriter = (StringWriter)_currentWriter.InnerWriters[0];

            //switch (_currentTask.Status)
            //{
            //    case TaskStatus.Faulted:
            //        var ex = _currentTask.Exception.InnerException;
            //        if (ex is JobFailedToStartException)
            //        {
            //            _currentInfoBuilder.Status = JobRunStatus.FailedToStart;
            //        }
            //        else
            //        {
            //            _currentInfoBuilder.Status = JobRunStatus.Failed;
            //        }

            //        _currentInfoBuilder.Exception = ex;

            //        break;

            //    case TaskStatus.RanToCompletion:
            //        _currentInfoBuilder.Status = JobRunStatus.Succeeded;
            //        break;

            //    case TaskStatus.Canceled:
            //        _currentInfoBuilder.Status = JobRunStatus.Canceled;
            //        break;

            //    default:
            //        _currentInfoBuilder.Status =
            //            JobRunStatus.Unknown; // actually, very strange, but we cannot throw here.
            //        break;
            //}

            //var runInfo = _currentInfoBuilder.Build();
            //_runs.Add(runInfo);

            //jobOutputWriter.Dispose();

            //_currentWriter.Dispose();
            //_currentWriter = null;

            //_currentInfoBuilder = null;

            //_currentTokenSource.Dispose();
            //_currentTokenSource = null;

            //_currentTask = null;
            //_currentEndTask = null;

            //this.UpdateScheduleDueTime(); // updated in FinalizeJobRun
        }

        internal bool WakeUp(JobStartReason startReason, CancellationToken token)
        {
            this.UpdateScheduleDueTime();

            lock (_marinaLock)
            {
                if (_runContext != null)
                {
                    throw new NotImplementedException();
                }

                _runContext = new RunContext(
                    _routine,
                    _parameter,
                    _progressTracker,
                    _output,
                    token,
                    this.CompletionCallback);

                _runContext.Run();

                return true;
            }

            //throw new NotImplementedException();

            //lock (_wakeUpLock)
            //{
            //    lock (_marinaLock)
            //    {
            //        if (_runContext == null)
            //        {
            //            _runContext = this.CreateRunContext(_output, token);
            //        }
            //        else
            //        {
            //            // todo: log
            //            return false;
            //        }
            //    }

            //    try
            //    {
            //        this.Start();
            //        return true;
            //    }
            //    catch (Exception ex)
            //    {
            //        // todo: log ex in Employee's log.
            //        return false;
            //    }
            //}




            //lock (_wakeUpLock)
            //{
            //    try
            //    {
            //        if (this.IsEnabled)
            //        {
            //            // go on
            //        }
            //        else
            //        {
            //            throw new NotImplementedException();
            //        }

            //        this.Start();

            //        switch (startReason)
            //        {
            //            case JobStartReason.ScheduleDueTime:
            //                return WakeUpResult.StartedBySchedule;

            //            case JobStartReason.OverriddenDueTime:
            //                return WakeUpResult.StartedByOverriddenDueTime;

            //            case JobStartReason.Force2:
            //                return WakeUpResult.StartedByForce;

            //            default:
            //                return WakeUpResult.Unknown;
            //        }

            //        // started successfully


            //        //lock (_marinaLock)
            //        //{
            //        //    _runContext = new RunContext(_output);
            //        //}
            //    }
            //    catch (InappropriateProlStateException)
            //    {
            //        // already started.
            //        throw new NotImplementedException();
            //    }
            //}

            //throw new NotImplementedException();
            //lock (_dataLock)
            //{
            //    try
            //    {
            //        this.Start(); // todo0000 BAAAD!!!
            //        _currentTask = this.InitJobRunContext(true, token);

            //        _overriddenDueTime = null;
            //        this.UpdateScheduleDueTime(); // updated in WakeUp

            //        _currentEndTask = _currentTask.ContinueWith(this.EndJob);

            //        switch (_currentInfoBuilder.StartReason)
            //        {
            //            case JobStartReason.ScheduleDueTime:
            //                return WakeUpResult.StartedBySchedule;

            //            case JobStartReason.OverriddenDueTime:
            //                return WakeUpResult.StartedByOverriddenDueTime;

            //            case JobStartReason.Force2:
            //                return WakeUpResult.UnexpectedlyStartedByForce;

            //            default:
            //                return WakeUpResult.Unknown;
            //        }
            //    }
            //    catch (InappropriateProlStateException)
            //    {
            //        return WakeUpResult.AlreadyRunning;
            //    }
            //    catch (ObjectDisposedException)
            //    {
            //        return WakeUpResult.AlreadyDisposed;
            //    }
            //}
        }

        private void CompletionCallback(JobRunInfoBuilder jobRunInfoBuilder)
        {
            throw new NotImplementedException();
        }

        //private RunContext CreateRunContext(
        //    TextWriter output,
        //    CancellationToken? token)
        //{
        //    return new RunContext(_routine, _parameter, _progressTracker, output, token);
        //}

        private Task InitJobRunContext(bool byDueTime, CancellationToken? token)
        {
            throw new NotImplementedException();

            //var jobWriter = new StringWriterWithEncoding(Encoding.UTF8);
            //var writers = new List<TextWriter>
            //{
            //    jobWriter,
            //};

            //if (_output != null)
            //{
            //    writers.Add(_output);
            //}

            //var now = TimeProvider.GetCurrent();

            //_currentWriter = new MultiTextWriterLab(Encoding.UTF8, writers);

            //if (token.HasValue)
            //{
            //    _currentTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token.Value);
            //}
            //else
            //{
            //    _currentTokenSource = new CancellationTokenSource();
            //}

            //JobStartReason reason;

            //if (byDueTime)
            //{
            //    reason = _overriddenDueTime.HasValue
            //        ? JobStartReason.OverriddenDueTime
            //        : JobStartReason.ScheduleDueTime;
            //}
            //else
            //{
            //    reason = JobStartReason.Force2;
            //}

            //_currentInfoBuilder = new JobRunInfoBuilder(
            //    _runIndex,
            //    reason,
            //    this.GetEffectiveDueTime(),
            //    _overriddenDueTime.HasValue,
            //    now,
            //    JobRunStatus.Unknown,
            //    jobWriter);

            //_runIndex++;

            //Task task;

            //try
            //{
            //    task = _routine(_parameter, _progressTracker, _currentWriter, _currentTokenSource.Token);
            //}
            //catch (Exception ex)
            //{
            //    // todo: wrong. _routine might throw exception intentionally.
            //    // todo: deal with completed tasks?
            //    var jobEx = new JobFailedToStartException(ex);
            //    task = Task.FromException(jobEx);
            //}

            //return task;
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

        public void Dispose()
        {
            lock (_marinaLock)
            {
                if (_isDisposed)
                {
                    return; // won't dispose twice.
                }

                if (_runContext != null)
                {
                    throw new NotImplementedException();
                }

                _isDisposed = true;
            }
        }
    }
}
