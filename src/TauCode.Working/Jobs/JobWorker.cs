using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Extensions;
using TauCode.Infrastructure.Time;
using TauCode.Working.Exceptions;

namespace TauCode.Working.Jobs
{
    // todo clean up
    internal class JobWorker : WorkerBase
    {
        #region Fields

        //private ISchedule _schedule;
        //private JobDelegate _routine;
        //private object _parameter;
        //private IProgressTracker _progressTracker;
        //private TextWriter _output;

        private StringWriterWithEncoding _currentRunTextWriter;
        private CancellationTokenSource _currentRunCancellationTokenSource;
        private JobRunInfoBuilder _currentJobRunResultBuilder;


        private readonly List<JobRunInfo> _log;

        //private Task _currentTask;

        private int _runIndex;

        private readonly Job _job;

        #endregion

        #region Constructor

        internal JobWorker(/*Func<object, TextWriter, CancellationToken, Task> taskCreator, object parameter*/)
        {
            // todo checks
            //_taskCreator = taskCreator;
            //_log = new List<JobRunInfo>();
            //_parameter = parameter;

            //_schedule = new NeverSchedule();
            //_routine = JobExtensions.IdleJobRoutine;

            _job = new Job(this);
        }

        #endregion

        #region Private

        private void EndTask(Task task)
        {
            var now = TimeProvider.GetCurrent();
            _currentJobRunResultBuilder.EndTime = now;
            _currentJobRunResultBuilder.Output = _currentRunTextWriter.ToString();

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

                //case TaskStatus.Created:
                //    break;
                //case TaskStatus.Running:
                //    break;
                //case TaskStatus.WaitingForActivation:
                //    break;
                //case TaskStatus.WaitingForChildrenToComplete:
                //    break;
                //case TaskStatus.WaitingToRun:
                //    break;

                default:
                    throw new ArgumentOutOfRangeException(); // todo.
            }


            _currentRunTextWriter.Dispose();
            _currentRunTextWriter = null;

            _currentRunCancellationTokenSource.Dispose();
            _currentRunCancellationTokenSource = null;

            var jobRunResult = _currentJobRunResultBuilder.Build();
            _log.Add(jobRunResult);

            _currentJobRunResultBuilder = null;

            this.Stop();



            //throw new NotImplementedException();
            //return Task.CompletedTask;
        }

        #endregion

        #region Overridden

        protected override void StartImpl()
        {
            throw new NotImplementedException();
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

        internal void ForceStart()
        {
            throw new NotImplementedException();
        }

        internal void DueTimeStart()
        {
            throw new NotImplementedException();
        }

        internal void CancelCurrentJobRun()
        {
            this.RequestControlLock(() =>
            {
                if (this.State != WorkerState.Running)
                {
                    throw new NotImplementedException();
                }

                _currentRunCancellationTokenSource.Cancel();
            });

            //throw new NotImplementedException();
        }

        internal Job GetJob() => _job;

        internal T RequestWithControlLock<T>(Func<T> func)
        {
            T value = default;
            this.RequestControlLock(() => value = func());
            return value;
        }

        #endregion
    }
}