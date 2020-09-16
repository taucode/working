using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Extensions.Lab;
using TauCode.Infrastructure.Time;
using TauCode.Working.Exceptions;
using TauCode.Working.Schedules;
using TauCode.Working.Workers;

namespace TauCode.Working.Jobs
{
    // todo clean up
    internal class Employee : WorkerBase
    {
        #region Fields

        private readonly Vice _vice;
        private readonly IJob _job;

        private ISchedule _schedule;
        private DateTimeOffset _effectiveDueTime;
        private DateTimeOffset? _overriddenDueTime;

        private JobDelegate _routine;
        private object _parameter;
        private IProgressTracker _progressTracker;
        private TextWriter _output;
        private MultiTextWriterLab _currentRunTextWriter;
        private CancellationTokenSource _currentRunCancellationTokenSource;
        private JobRunInfoBuilder _currentJobRunResultBuilder;
        private readonly List<JobRunInfo> _runs;
        private int _runIndex;

        #endregion

        #region Constructor

        internal Employee(Vice vice)
        {
            _vice = vice;
            _job = new Job(this);

            _schedule = NeverSchedule.Instance;

            _routine = JobExtensions.IdleJobRoutine;
            _runs = new List<JobRunInfo>();

            this.UpdateEffectiveDueTime();
        }

        #endregion

        #region Private

        private void CheckStopped(string preamble)
        {
            this.CheckNotDisposed();
            this.CheckState(preamble, WorkerState.Stopped);
        }

        /// <summary>
        /// Should always be called from 'InvokeWithControlLock'
        /// </summary>
        private void CheckNotDisposed()
        {
            if (this.State == WorkerState.Disposed)
            {
                throw new JobObjectDisposedException($"{nameof(IJob)} '{this.Name}'");
            }
        }

        private void EndJob(Task task)
        {
            var now = TimeProvider.GetCurrent();
            _currentJobRunResultBuilder.EndTime = now;

            //var stringWriter = (StringWriterWithEncoding)_currentRunTextWriter.InnerWriters[0];
            //_currentJobRunResultBuilder.Output = stringWriter.ToString();

            switch (task.Status)
            {
                case TaskStatus.Faulted:
                    var ex = task.Exception.InnerException;
                    if (ex is JobFailedToStartException)
                    {
                        _currentJobRunResultBuilder.Status = JobRunStatus.FailedToStart;
                    }
                    else
                    {
                        _currentJobRunResultBuilder.Status = JobRunStatus.Faulted;
                    }

                    _currentJobRunResultBuilder.Exception = ex;

                    break;

                case TaskStatus.RanToCompletion:
                    _currentJobRunResultBuilder.Status = JobRunStatus.Succeeded;
                    break;

                case TaskStatus.Canceled:
                    _currentJobRunResultBuilder.Status = JobRunStatus.Canceled;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(); // todo.
            }

            _currentJobRunResultBuilder.OutputWriter.Dispose();

            //stringWriter.Dispose();
            _currentRunTextWriter.Dispose();
            _currentRunTextWriter = null;

            _currentRunCancellationTokenSource.Dispose();
            _currentRunCancellationTokenSource = null;

            var jobRunResult = _currentJobRunResultBuilder.Build();
            _runs.Add(jobRunResult);
            _currentJobRunResultBuilder = null;

            this.InvokeWithControlLock(() =>
            {
                // may be in disposing process already.
                if (this.State == WorkerState.Running)
                {
                    this.Stop();
                }
            });

            //// it might appear task was cancelled due to a dispose request.
            //var state = this.State;
            //if (state.IsIn(WorkerState.Dispo-sing, WorkerState.Disposed))
            //{
            //    return;
            //}

            //// stop worker.
            //try
            //{
            //    this.Sto-p();
            //}
            //catch
            //{
            //    // todo: catch 'InvalidWorkerStateException', which will be thrown by WorkerBase.CheckState.
            //}
        }

        private void UpdateEffectiveDueTime()
        {
            var now = TimeProvider.GetCurrent();
            _effectiveDueTime = _overriddenDueTime ?? _schedule.GetDueTimeAfter(now);
        }

        //private T GetIfNotDisposed<T>(Func<T> getter)
        //{
        //    return this.GetWithControlLock(() =>
        //    {
        //        this.CheckNotDisposed();
        //        var value = getter();
        //        return value;
        //    });
        //}

        //private void InvokeIfNotDisposed(Action action)
        //{
        //    this.InvokeWithControlLock(() =>
        //    {
        //        this.CheckNotDisposed();
        //        action();
        //    });
        //}

        #endregion

        #region Overridden

        protected override void StartImpl()
        {
            this.ChangeState(WorkerState.Running);
        }

        protected override void PauseImpl()
        {
            throw new NotSupportedException("Pausing is not supported.");
        }

        protected override void ResumeImpl()
        {
            throw new NotSupportedException("Resuming is not supported.");
        }

        protected override void StopImpl()
        {
            this.ChangeState(WorkerState.Stopped);
        }

        protected override void DisposeImpl()
        {
            if (this.State == WorkerState.Running)
            {
                _currentRunCancellationTokenSource.Cancel();
            }

            this.ChangeState(WorkerState.Disposed);
        }

        #endregion

        #region Internal

        //internal void ForceStart()
        //{
        //    throw new NotImplementedException();
        //    //var jobStartResult = this.StartJob(StartReason.Force, _vice.GetDueTimeInfo(this.Name));
        //    //throw new NotImplementedException();
        //}



        #endregion

        #region Internal - called by Vice

        internal IJob GetJob() => _job;

        #endregion

        #region Internal - called by IJob (can throw)

        internal ISchedule GetSchedule()
        {
            return this.GetWithControlLock(() =>
            {
                this.CheckNotDisposed();
                return _schedule;
            });
        }

        internal void UpdateSchedule(ISchedule schedule)
        {
            this.InvokeWithControlLock(() =>
            {
                this.CheckNotDisposed();
                _schedule = schedule;
                _vice.OnScheduleChanged();
            });

        }

        internal JobDelegate GetRoutine()
        {
            return this.GetWithControlLock(() =>
            {
                this.CheckNotDisposed();
                return _routine;
            });
        }

        internal void SetRoutine(JobDelegate routine)
        {
            this.InvokeWithControlLock(() =>
            {
                this.CheckStopped($"Set '{nameof(IJob.Routine)}' requested.");

                _routine = routine ?? throw new ArgumentNullException(nameof(routine));
            });
        }

        internal object GetParameter()
        {
            return this.GetWithControlLock(() =>
            {
                this.CheckNotDisposed();
                return _parameter;
            });
        }

        internal void SetParameter(object parameter)
        {
            this.InvokeWithControlLock(() =>
            {
                this.CheckStopped($"Set '{nameof(IJob.Parameter)}' requested.");

                _parameter = parameter; // can be null
            });
        }

        internal IProgressTracker GetProgressTracker()
        {
            return this.GetWithControlLock(() =>
            {
                this.CheckNotDisposed();
                return _progressTracker;
            });
        }

        internal void SetProgressTracker(IProgressTracker progressTracker)
        {
            this.InvokeWithControlLock(() =>
            {
                this.CheckStopped($"Set '{nameof(IJob.ProgressTracker)}' requested.");

                _progressTracker = progressTracker ?? throw new ArgumentNullException(nameof(progressTracker));
            });
        }

        internal TextWriter GetOutput()
        {
            return this.GetWithControlLock(() =>
            {
                this.CheckNotDisposed();
                return _output;
            });
        }

        internal void SetOutput(TextWriter output)
        {
            this.InvokeWithControlLock(() =>
            {
                this.CheckStopped($"Set '{nameof(IJob.Output)}' requested.");

                _output = output ?? throw new ArgumentNullException(nameof(output));
            });
        }

        internal JobInfo GetJobInfo(int? maxRunCount)
        {
            throw new NotImplementedException();

            //if (maxRunCount.HasValue && maxRunCount.Value < 0)
            //{
            //    throw new ArgumentOutOfRangeException(nameof(maxRunCount));
            //}

            //var jobInfo = this.GetWithControlLock(() =>
            //{
            //    this.CheckNotDisposed();

            //    var jobInfoBuilder = new JobInfoBuilder();
            //    var dueTimeInfo = _vice.GetNextDueTimeInfo(this.Name);
            //    jobInfoBuilder.DueTimeInfo = dueTimeInfo;
            //    jobInfoBuilder.RunCount = _runIndex;

            //    var runCountToTake = Math.Min(
            //        _runs.Count,
            //        maxRunCount ?? int.MaxValue);

            //    for (var i = 0; i < runCountToTake; i++)
            //    {
            //        jobInfoBuilder.Runs.Add(_runs[i]);
            //    }

            //    return jobInfoBuilder.Build();
            //});

            //return jobInfo;
        }

        #endregion

        #region Internal - interface to Vice

        internal void OverrideDueTime(DateTimeOffset? dueTime) => _vice.OverrideDueTime(this.Name, dueTime);

        #endregion


        



        //internal JobStartResult StartJob(JobStartReason startReason, DueTimeInfo dueTimeInfo)
        //{
        //    var jobStartResult = this.GetWithControlLock(() =>
        //    {
        //        if (this.WorkerIsDisposed())
        //        {
        //            return JobStartResult.AlreadyDisposed;
        //        }

        //        if (this.WorkerIsRunning())
        //        {
        //            switch (_currentJobRunResultBuilder.StartReason)
        //            {
        //                case JobStartReason.DueTime:
        //                    return JobStartResult.AlreadyStartedByDueTime;

        //                case JobStartReason.Force:
        //                    return JobStartResult.AlreadyStartedByForce;

        //                default:
        //                    return JobStartResult.Unknown; // should never happen, it's an error.
        //            }
        //        }

        //        if (!this.WorkerIsStopped())
        //        {
        //            return JobStartResult.Unknown; // should never happen, it's an error.
        //        }

        //        var startTime = TimeProvider.GetCurrent();
        //        _currentJobRunResultBuilder = new JobRunInfoBuilder(_runIndex, startReason, dueTimeInfo, startTime);
        //        _currentRunCancellationTokenSource = new CancellationTokenSource();

        //        var writers = new List<TextWriter>();
        //        var runWriter = new StringWriterWithEncoding(Encoding.UTF8);
        //        writers.Add(runWriter);

        //        if (_output != null)
        //        {
        //            writers.Add(_output);
        //        }

        //        _currentRunTextWriter = new MultiTextWriterLab(Encoding.UTF8, writers);

        //        Task task;

        //        try
        //        {
        //            task = _routine(
        //                _parameter,
        //                _progressTracker,
        //                _currentRunTextWriter,
        //                _currentRunCancellationTokenSource.Token);
        //        }
        //        catch (Exception ex)
        //        {
        //            var jobEx = new JobFailedToStartException(ex);
        //            task = Task.FromException(jobEx);
        //        }

        //        _runIndex++;
        //        this.Start();

        //        task.ContinueWith(this.EndJob);
        //        return JobStartResult.Started;
        //    });

        //    return jobStartResult;
        //}

        

        internal void GetFired()
        {
            this.InvokeWithControlLock(() =>
            {
                if (this.WorkerIsDisposed())
                {
                    return;
                }

                _vice.FireMe(this.Name);
                this.Dispose();
            });
        }

        public DateTimeOffset? GetDueTimeForVice() => this.WorkerIsDisposed() ? (DateTimeOffset?)null : _effectiveDueTime;

        public void WakeUp()
        {
            throw new NotImplementedException();
        }
    }
}
