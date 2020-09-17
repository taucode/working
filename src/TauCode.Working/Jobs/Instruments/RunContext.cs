using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Extensions;
using TauCode.Extensions.Lab;
using TauCode.Infrastructure.Time;

// todo clean
namespace TauCode.Working.Jobs.Instruments
{
    internal class RunContext //: IDisposable
    {
        private readonly StringWriterWithEncoding _systemWriter;

        private readonly JobRunsHolder _runsHolder;

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
            JobRunsHolder runsHolder,
            DueTimeHolder dueTimeHolder,
            JobStartReason startReason,
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

            _runsHolder = runsHolder;

            var dueTimeInfo = dueTimeHolder.GetDueTimeInfo();
            var dueTime = dueTimeInfo.GetEffectiveDueTime();
            var dueTimeWasOverridden = dueTimeInfo.IsDueTimeOverridden();

            _runInfoBuilder = new JobRunInfoBuilder(
                _runsHolder.Count,
                startReason,
                dueTime,
                dueTimeWasOverridden,
                startTime,
                JobRunStatus.Running,
                _systemWriter);

            _runsHolder.Start(_runInfoBuilder.Build());
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
            Exception exception = null;

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
                    exception = ExtractTaskException(task.Exception);
                    break;

                default:
                    status = JobRunStatus.Unknown;
                    break;
            }

            _runInfoBuilder.EndTime = now;
            _runInfoBuilder.Status = status;
            _runInfoBuilder.Exception = exception;

            var jobRunInfo = _runInfoBuilder.Build();
            _runsHolder.Finish(jobRunInfo);
        }

        private static Exception ExtractTaskException(AggregateException taskException)
        {
            return taskException?.InnerException ?? taskException;
        }

        internal void Dispose()
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
}
