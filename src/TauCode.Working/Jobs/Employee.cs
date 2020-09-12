using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Extensions;
using TauCode.Extensions.Lab;
using TauCode.Working.Exceptions;

namespace TauCode.Working.Jobs
{
    // todo clean up
    internal class Employee : WorkerBase
    {
        #region Fields

        private JobDelegate _routine;
        private object _parameter;
        private IProgressTracker _progressTracker;
        private TextWriter _output;
        private readonly Vice _vice;
        private MultiTextWriterLab _currentRunTextWriter;
        private CancellationTokenSource _currentRunCancellationTokenSource;
        private JobRunInfoBuilder _currentJobRunResultBuilder;
        private readonly List<JobRunInfo> _runs;
        private int _runIndex;
        private readonly IJob _job;
        private readonly object _lock;

        #endregion

        #region Constructor

        internal Employee(Vice vice)
        {
            _vice = vice;
            _routine = JobExtensions.IdleJobRoutine;
            _job = new Job(this);
            _runs = new List<JobRunInfo>();
            _lock = new object();
        }

        #endregion

        #region Private

        private void CheckStopped(string preamble)
        {
            this.CheckState2(preamble, WorkerState.Stopped);
        }

        private void EndTask(Task task)
        {
            var now = _vice.GetCurrentTime();
            _currentJobRunResultBuilder.EndTime = now;

            var stringWriter = (StringWriterWithEncoding)_currentRunTextWriter.InnerWriters[0];
            _currentJobRunResultBuilder.Output = stringWriter.ToString();

            switch (task.Status)
            {
                case TaskStatus.Faulted:
                    var ex = task.Exception.InnerException;
                    if (ex is JobRunFailedToStartException)
                    {
                        _currentJobRunResultBuilder.Status = JobRunStatus.FailedToStart;
                    }
                    else
                    {
                        _currentJobRunResultBuilder.Status = JobRunStatus.Failed;
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

            stringWriter.Dispose();
            _currentRunTextWriter.Dispose();
            _currentRunTextWriter = null;

            _currentRunCancellationTokenSource.Dispose();
            _currentRunCancellationTokenSource = null;

            var jobRunResult = _currentJobRunResultBuilder.Build();
            _runs.Add(jobRunResult);
            _currentJobRunResultBuilder = null;

            this.Stop();
        }

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
            this.ChangeState(WorkerState.Disposed);
        }

        #endregion

        #region Internal

        internal void ForceStart() => this.StartJob(StartReason.Force, _vice.GetDueTimeInfo(this.Name));

        internal void DueTimeStart()
        {
            throw new NotImplementedException();
        }

        internal void CancelCurrentJobRun()
        {
            this.InvokeWithControlLock(() =>
            {
                if (this.State != WorkerState.Running)
                {
                    throw new NotImplementedException();
                }

                _currentRunCancellationTokenSource.Cancel();
            });
        }

        internal IJob GetJob() => _job;

        #endregion

        internal JobInfo GetJobInfo(int? maxRunCount)
        {
            // todo: check null or non-negative 'maxRunCount'

            // todo: _runIndex is guarded by control lock in other places!
            lock (_lock)
            {
                var jobInfoBuilder = new JobInfoBuilder(this.Name);
                var dueTimeInfo = _vice.GetDueTimeInfo(this.Name);
                jobInfoBuilder.DueTimeInfo = dueTimeInfo;
                jobInfoBuilder.RunCount = _runIndex;

                var runCountToTake = Math.Min(
                    _runs.Count,
                    maxRunCount ?? int.MaxValue);

                for (var i = 0; i < runCountToTake; i++)
                {
                    jobInfoBuilder.Runs.Add(_runs[i]);
                }

                return jobInfoBuilder.Build();
            }
        }

        internal ISchedule GetSchedule() => _vice.GetSchedule(this.Name); // todo: check resharper warning disappeared.
        //{
        //    // todo: lock with _scheduleLock
        //    ISchedule result = default;

        //    this.InvokeWithControlLock(() =>
        //    {
        //        result = _schedule;
        //    });

        //    return result;
        //}

        //internal void SetSchedule(ISchedule schedule)
        //{
        //    // todo: lock with _scheduleLock
        //    this.InvokeWithControlLock(() =>
        //    {
        //        this.CheckStopped($"Set '{nameof(IJob.Schedule)}' requested.");

        //        _schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
        //    });
        //}

        internal TextWriter GetOutput()
        {
            TextWriter result = default;

            this.InvokeWithControlLock(() =>
            {
                result = _output;
            });

            return result;
        }

        internal void SetOutput(TextWriter output)
        {
            this.InvokeWithControlLock(() =>
            {
                this.CheckStopped($"Set '{nameof(IJob.Output)}' requested.");

                _output = output ?? throw new ArgumentNullException(nameof(output));
            });
        }

        internal IProgressTracker GetProgressTracker()
        {
            IProgressTracker result = default;

            this.InvokeWithControlLock(() =>
            {
                result = _progressTracker;
            });

            return result;
        }

        internal void SetProgressTracker(IProgressTracker progressTracker)
        {
            this.InvokeWithControlLock(() =>
            {
                this.CheckStopped($"Set '{nameof(IJob.ProgressTracker)}' requested.");

                _progressTracker = progressTracker ?? throw new ArgumentNullException(nameof(progressTracker));
            });
        }

        internal object GetParameter()
        {
            object result = default;

            this.InvokeWithControlLock(() =>
            {
                result = _parameter;
            });

            return result;
        }

        internal void SetParameter(object parameter)
        {
            this.InvokeWithControlLock(() =>
            {
                this.CheckStopped($"Set '{nameof(IJob.Parameter)}' requested.");

                _parameter = parameter; // can be null
            });
        }

        internal JobDelegate GetRoutine()
        {
            JobDelegate result = default;

            this.InvokeWithControlLock(() =>
            {
                result = _routine;
            });

            return result;
        }

        internal void SetRoutine(JobDelegate routine)
        {
            this.InvokeWithControlLock(() =>
            {
                this.CheckStopped($"Set '{nameof(IJob.Routine)}' requested.");

                _routine = routine ?? throw new ArgumentNullException(nameof(routine));
            });
        }

        internal void OverrideDueTime(DateTime? dueTime) => _vice.OverrideDueTime(this.Name, dueTime);
        
        internal void StartJob(StartReason startReason, DueTimeInfo dueTimeInfo)
        {
            this.InvokeWithControlLock(() =>
            {
                this.CheckState2("Job force start requested.", WorkerState.Stopped);

                var startTime = _vice.GetCurrentTime();
                _currentJobRunResultBuilder = new JobRunInfoBuilder(_runIndex, startReason, dueTimeInfo, startTime);
                _currentRunCancellationTokenSource = new CancellationTokenSource();

                var writers = new List<TextWriter>();
                var runWriter = new StringWriterWithEncoding(Encoding.UTF8);
                writers.Add(runWriter);

                if (_output != null)
                {
                    writers.Add(_output);
                }

                _currentRunTextWriter = new MultiTextWriterLab(Encoding.UTF8, writers);

                Task task;

                try
                {
                    task = _routine(
                        _parameter,
                        _progressTracker,
                        _currentRunTextWriter,
                        _currentRunCancellationTokenSource.Token);
                }
                catch (Exception ex)
                {
                    var jobEx = new JobRunFailedToStartException(ex);
                    task = Task.FromException(jobEx);
                }

                _runIndex++;
                this.Start();

                task.ContinueWith(this.EndTask);
            });
        }

        internal bool UpdateSchedule(ISchedule schedule)
        {
            // todo: check not disposed, here & anywhere

            _vice.UpdateSchedule(this.Name, schedule);
            var willApply = this.State != WorkerState.Running;
            return willApply;
        }
    }
}
