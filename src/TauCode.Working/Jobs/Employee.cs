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
using TauCode.Working.Jobs.Schedules;

namespace TauCode.Working.Jobs
{
    // todo clean up
    internal class Employee : WorkerBase
    {
        #region Fields

        //private ISchedule _schedule;
        //private JobDelegate _routine;
        //private object _parameter;
        //private IProgressTracker _progressTracker;
        //private TextWriter _output;

        private ISchedule _schedule;
        private JobDelegate _routine;
        private object _parameter;
        private IProgressTracker _progressTracker;
        private TextWriter _output;

        private readonly Vice _vice;

        //private StringWriterWithEncoding _currentRunTextWriter;
        private MultiTextWriterLab _currentRunTextWriter;
        private CancellationTokenSource _currentRunCancellationTokenSource;
        private JobRunInfoBuilder _currentJobRunResultBuilder;

        //private readonly DueTimeInfoBuilder _dueTimeInfoBuilder;

        private readonly List<JobRunInfo> _runs;

        //private Task _currentTask;

        private int _runIndex;

        private readonly IJob _job;

        private readonly object _lock;
        //private bool _isEnabled;

        #endregion

        #region Constructor

        internal Employee(Vice vice)
        {
            _vice = vice;

            _schedule = new NeverSchedule();
            _routine = JobExtensions.IdleJobRoutine;

            _job = new Job(this);
            _runs = new List<JobRunInfo>();

            //_dueTimeInfoBuilder = new DueTimeInfoBuilder();
            //_dueTimeInfoBuilder.UpdateBySchedule(_job.Schedule);



            _lock = new object();
            //_isEnabled = true;
        }

        #endregion

        #region Private

        private void EndTask(Task task)
        {
            var now = TimeProvider.GetCurrent();
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

            //var now = TimeProvider.GetCurrent(); // todo checks utc

            //_currentRunCancellationTokenSource = new CancellationTokenSource();
            //_currentRunTextWriter = new StringWriterWithEncoding(Encoding.UTF8);

            //// todo0
            ////_currentJobRunResultBuilder = new JobRunInfoBuilder(_runIndex, now);

            //// todo try/catch

            //Task task;

            //try
            //{
            //    task = _taskCreator(_parameter, _currentRunTextWriter, _currentRunCancellationTokenSource.Token);
            //}
            //catch (Exception ex)
            //{
            //    var jobEx = new JobRunFailedToStartException(ex);
            //    task = Task.FromException(jobEx);
            //}

            //_runIndex++;

            //task.ContinueWith(this.EndTask);

            ////_currentTask = _taskCreator(_currentRunTextWriter, _currentRunCancellationTokenSource.Token);
            ////_currentTask.ContinueWith(this.EndTask);

            //this.ChangeState(WorkerState.Running);
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

        //internal DueTimeInfoBuilder getDueTimeInfoBuilder() => _dueTimeInfoBuilder;

        internal void ForceStart()
        {
            this.InvokeWithControlLock(() =>
            {
                this.CheckState2("Job force start requested.", WorkerState.Stopped);

                var dueTimeInfo = _vice.GetDueTimeInfo(this.Name);
                var startTime = TimeProvider.GetCurrent();
                _currentJobRunResultBuilder = new JobRunInfoBuilder(_runIndex, StartReason.Force, dueTimeInfo, startTime);
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

            //throw new NotImplementedException();
        }

        internal IJob GetJob() => _job;

        //internal T GetWithControlLock<T>(Func<T> func)
        //{
        //    T value = default;
        //    this.InvokeWithControlLock(() => value = func());
        //    return value;
        //}

        //internal JobInfoBuilder GetJobInfoBuilder(int? maxRunCount)
        //{
        //    var builder = new JobInfoBuilder(this.Name)
        //    {
        //        IsEnabled = this.IsEnabled,
        //    };

        //    //this.RequestControlLock(() =>
        //    //{
        //    //    builder.DueTimeInfo = _dueTimeInfoBuilder.Build();
        //    //});

        //    return builder;
        //}

        //internal bool IsEnabled
        //{
        //    get
        //    {
        //        lock (_lock)
        //        {
        //            return _isEnabled;
        //        }
        //    }
        //    set
        //    {
        //        lock (_lock)
        //        {
        //            _isEnabled = value;
        //        }
        //    }
        //}

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

        internal ISchedule GetSchedule()
        {
            lock (_lock)
            {
                return _schedule;
            }
        }

        internal void SetSchedule(ISchedule value)
        {
            throw new NotImplementedException();
        }

        internal TextWriter GetOutput()
        {
            lock (_lock)
            {
                return _output;
            }
        }

        internal void SetOutput(TextWriter value)
        {
            throw new NotImplementedException();
        }

        internal IProgressTracker GetProgressTracker()
        {
            lock (_lock)
            {
                return _progressTracker;
            }
        }

        internal void SetProgressTracker(IProgressTracker value)
        {
            throw new NotImplementedException();
        }

        internal object GetParameter()
        {
            lock (_lock)
            {
                return _parameter;
            }
        }

        internal void SetParameter(object value)
        {
            throw new NotImplementedException();
        }

        internal JobDelegate GetRoutine()
        {
            lock (_lock)
            {
                return _routine;
            }
        }

        internal void SetRoutine(JobDelegate value)
        {
            throw new NotImplementedException();
        }

        internal void OverrideDueTime(DateTime? dueTime)
        {
            lock (_lock)
            {
                _vice.OverrideDueTime(this.Name, dueTime, _schedule);
            }
        }

        //internal void Enable(in bool enable)
        //{
        //    lock (_lock)
        //    {
        //        _isEnabled = enable;
        //    }
        //}
    }
}