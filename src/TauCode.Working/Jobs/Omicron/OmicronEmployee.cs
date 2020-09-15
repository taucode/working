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

        private readonly object _lock;

        internal OmicronEmployee(OmicronVice vice)
        {
            _vice = vice;
            _job = new OmicronJob(this);
            _schedule = NeverSchedule.Instance;
            _routine = JobExtensions.IdleJobRoutine;
            _runs = new List<JobRunInfo>();

            _lock = new object();

            this.UpdateScheduleDueTime();
        }

        private void UpdateScheduleDueTime()
        {
            var now = TimeProvider.GetCurrent();

            lock (_lock)
            {
                _scheduleDueTime = _schedule.GetDueTimeAfter(now.AddTicks(1));
            }
        }

        private DateTimeOffset GetEffectiveDueTime()
        {
            lock (_lock)
            {
                return _overriddenDueTime ?? _scheduleDueTime;
            }
        }

        internal IJob GetJob() => _job;

        internal ISchedule Schedule
        {
            get
            {
                lock (_lock)
                {
                    return _schedule;
                }
            }
            set
            {
                lock (_lock)
                {
                    if (this.IsDisposed)
                    {
                        throw new NotImplementedException();
                    }

                    _schedule = value;
                    this.UpdateScheduleDueTime();
                    _vice.OnScheduleChanged();
                }
            }
        }

        // todo: what about disposal, here & anywhere?
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

        internal bool IsEnabled
        {
            get
            {
                lock (_lock)
                {
                    return _isEnabled;
                }
            }
            set
            {
                lock (_lock)
                {
                    _isEnabled = value;
                    _vice.OnScheduleChanged();
                }
            }
        }

        internal DateTimeOffset? GetDueTimeForVice()
        {
            if (this.IsDisposed)
            {
                return null;
            }

            return this.GetEffectiveDueTime();
        }

        private void EndJob(Task task) => this.Stop(false);
        //{
        //    lock (_lock)
        //    {
        //        this.FinalizeJobRun(task);
        //        this.Stop(false);
        //    }
        //}

        protected override void OnStopped()
        {
            lock (_lock)
            {
                if (_currentEndTask != null)
                {
                    //_currentEndTask.Wait(100); // todo const
                    this.FinalizeJobRun();
                }
            }
        }

        private void FinalizeJobRun(/*Task task*/)
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
                    _currentInfoBuilder.Status = JobRunStatus.Unknown; // actually, very strange, but we cannot throw here.
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
        }

        internal WakeUpResult WakeUp(CancellationToken token)
        {
            lock (_lock)
            {
                try
                {
                    this.Start();
                    _currentTask = this.InitJobRunContext(token);

                    _overriddenDueTime = null;
                    this.UpdateScheduleDueTime();

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

            //try
            //{
            //    lock (_lock)
            //    {
            //        this.Start();

            //        var writers = new List<TextWriter>
            //        {
            //            new StringWriterWithEncoding(Encoding.UTF8),
            //        };

            //        if (_output != null)
            //        {
            //            writers.Add(_output);
            //        }

            //        var now = TimeProvider.GetCurrent();

            //        _currentRunTextWriter = new MultiTextWriterLab(Encoding.UTF8, writers);
            //        _currentJobRunInfoBuilder = new JobRunInfoBuilder(
            //            _runIndex,
            //            _overriddenDueTime.HasValue? JobStartReason.OverriddenDueTime : JobStartReason.ScheduleDueTime,
            //            now);

            //        _runIndex++;

            //        Task task;

            //        try
            //        {
            //            task = _routine(_parameter, _progressTracker, _currentRunTextWriter, token);
            //        }
            //        catch (Exception ex)
            //        {
            //            task = Task.FromException(ex);
            //        }
            //    }
            //}
            //catch (InappropriateProlStateException)
            //{
            //    // todo: was started (check)

            //}
            //catch (ObjectDisposedException)
            //{
            //    return WakeUpResult.WasDisposed;
            //}
        }

        private Task InitJobRunContext(CancellationToken token)
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
            _currentTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            _currentInfoBuilder = new JobRunInfoBuilder(
                _runIndex,
                _overriddenDueTime.HasValue ? JobStartReason.OverriddenDueTime : JobStartReason.ScheduleDueTime,
                this.GetEffectiveDueTime(),
                _overriddenDueTime.HasValue,
                now,
                JobRunStatus.Running,
                jobWriter);

            _runIndex++;

            Task task;

            try
            {
                task = _routine(_parameter, _progressTracker, _currentWriter, _currentTokenSource.Token);
            }
            catch (Exception ex)
            {
                var jobEx = new JobFailedToStartException(ex);
                task = Task.FromException(jobEx);
            }

            return task;
        }

        internal JobInfo GetInfo(int? maxRunCount)
        {
            lock (_lock)
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

        internal void Cancel()
        {
            throw new NotImplementedException();
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
