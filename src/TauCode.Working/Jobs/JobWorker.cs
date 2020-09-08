using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Extensions;
using TauCode.Infrastructure.Time;
using TauCode.Working.Exceptions;

namespace TauCode.Working.Jobs
{
    internal class JobWorker : WorkerBase
    {
        private readonly Func<TextWriter, CancellationToken, Task<bool>> _taskCreator;

        private StringWriterWithEncoding _currentRunTextWriter;
        private CancellationTokenSource _currentRunCancellationTokenSource;
        private JobRunResultBuilder _currentJobRunResultBuilder;

        //private Task _currentTask;

        private int _runIndex;

        internal JobWorker(Func<TextWriter, CancellationToken, Task<bool>> taskCreator)
        {
            // todo checks
            _taskCreator = taskCreator;
        }

        protected override void StartImpl()
        {
            var now = TimeProvider.GetCurrent(); // todo checks utc

            _currentRunCancellationTokenSource = new CancellationTokenSource();
            _currentRunTextWriter = new StringWriterWithEncoding(Encoding.UTF8);
            _currentJobRunResultBuilder = new JobRunResultBuilder(_runIndex, now);

            // todo try/catch

            Task task;

            try
            {
                task = _taskCreator(_currentRunTextWriter, _currentRunCancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                var jobEx = new JobRunFailedToStartException(ex);
                task = Task.FromException(jobEx);
            }

            _runIndex++;

            task.ContinueWith(this.EndTask);

            //_currentTask = _taskCreator(_currentRunTextWriter, _currentRunCancellationTokenSource.Token);
            //_currentTask.ContinueWith(this.EndTask);

            this.ChangeState(WorkerState.Running);
        }

        private void EndTask(Task task)
        {
            var now = TimeProvider.GetCurrent();
            _currentJobRunResultBuilder.FinishedAt = now;
            _currentJobRunResultBuilder.Output = _currentRunTextWriter.ToString();

            var boolTask = (Task<bool>)task;
            
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

                //case TaskStatus.Canceled:
                //    break;
                //case TaskStatus.Created:
                //    break;
                //case TaskStatus.RanToCompletion:
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
                    throw new ArgumentOutOfRangeException();
            }

            this.Stop();

            throw new NotImplementedException();
            //return Task.CompletedTask;
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
    }
}
