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

        private ISchedule _schedule;
        private DateTimeOffset _effectiveDueTime;
        private DateTimeOffset? _overriddenDueTime;

        private JobDelegate _routine;
        private object _parameter;
        private IProgressTracker _progressTracker;
        private TextWriter _output;

        private MultiTextWriterLab _currentRunTextWriter;
        private JobRunInfoBuilder _currentJobRunInfoBuilder;
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

            this.UpdateEffectiveDueTime();
        }

        private void UpdateEffectiveDueTime()
        {
            var now = TimeProvider.GetCurrent();
            _effectiveDueTime = _overriddenDueTime ?? _schedule.GetDueTimeAfter(now);
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
                    this.UpdateEffectiveDueTime();
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

        internal DateTimeOffset? GetDueTimeForVice()
        {
            if (this.IsDisposed)
            {
                return null;
            }

            lock (_lock)
            {
                return _effectiveDueTime;
            }
        }

        private void EndJob(Task task)
        {
            throw new NotImplementedException();
            //var now = TimeProvider.GetCurrent();
            //_currentJobRunResultBuilder.EndTime = now;

            //var stringWriter = (StringWriterWithEncoding)_currentRunTextWriter.InnerWriters[0];
            //_currentJobRunResultBuilder.Output = stringWriter.ToString();

            //switch (task.Status)
            //{
            //    case TaskStatus.Faulted:
            //        var ex = task.Exception.InnerException;
            //        if (ex is JobFailedToStartException)
            //        {
            //            _currentJobRunResultBuilder.Status = JobRunStatus.FailedToStart;
            //        }
            //        else
            //        {
            //            _currentJobRunResultBuilder.Status = JobRunStatus.Failed;
            //        }

            //        _currentJobRunResultBuilder.Exception = ex;

            //        break;

            //    case TaskStatus.RanToCompletion:
            //        _currentJobRunResultBuilder.Status = JobRunStatus.Succeeded;
            //        break;

            //    case TaskStatus.Canceled:
            //        _currentJobRunResultBuilder.Status = JobRunStatus.Canceled;
            //        break;

            //    default:
            //        throw new ArgumentOutOfRangeException(); // todo.
            //}

            //stringWriter.Dispose();
            //_currentRunTextWriter.Dispose();
            //_currentRunTextWriter = null;

            //_currentRunCancellationTokenSource.Dispose();
            //_currentRunCancellationTokenSource = null;

            //var jobRunResult = _currentJobRunResultBuilder.Build();
            //_runs.Add(jobRunResult);
            //_currentJobRunResultBuilder = null;

            //this.InvokeWithControlLock(() =>
            //{
            //    // may be in disposing process already.
            //    if (this.State == WorkerState.Running)
            //    {
            //        this.Stop();
            //    }
            //});

            ////// it might appear task was cancelled due to a dispose request.
            ////var state = this.State;
            ////if (state.IsIn(WorkerState.Dispo-sing, WorkerState.Disposed))
            ////{
            ////    return;
            ////}

            ////// stop worker.
            ////try
            ////{
            ////    this.Sto-p();
            ////}
            ////catch
            ////{
            ////    // todo: catch 'InvalidWorkerStateException', which will be thrown by WorkerBase.CheckState.
            ////}
        }

        internal WakeUpResult WakeUp(CancellationToken token)
        {
            lock (_lock)
            {
                try
                {
                    this.Start();
                    var task = this.InitJobRunContext(token);

                    task.ContinueWith(this.EndJob);

                    switch (_currentJobRunInfoBuilder.StartReason)
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
            var writers = new List<TextWriter>
            {
                new StringWriterWithEncoding(Encoding.UTF8),
            };

            if (_output != null)
            {
                writers.Add(_output);
            }

            var now = TimeProvider.GetCurrent();

            _currentRunTextWriter = new MultiTextWriterLab(Encoding.UTF8, writers);
            _currentJobRunInfoBuilder = new JobRunInfoBuilder(
                _runIndex,
                _overriddenDueTime.HasValue ? JobStartReason.OverriddenDueTime : JobStartReason.ScheduleDueTime,
                now)
            {
                Status = JobRunStatus.Running
            };

            _runIndex++;

            Task task;

            try
            {
                task = _routine(_parameter, _progressTracker, _currentRunTextWriter, token);
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
