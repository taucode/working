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
    internal class RunContext
    {
        #region Fields

        private readonly Runner _initiator;

        private readonly CancellationTokenSource _tokenSource;
        private readonly JobRunInfoBuilder _runInfoBuilder;
        private readonly StringWriterWithEncoding _systemWriter;

        private readonly Task _task;

        private readonly ObjectLogger _logger;

        #endregion

        #region Constructor

        internal RunContext(
            Runner initiator,
            JobStartReason startReason,
            CancellationToken? token)
        {
            _initiator = initiator;
            var jobProperties = _initiator.JobPropertiesHolder.ToJobProperties();

            _tokenSource = token.HasValue ?
                CancellationTokenSource.CreateLinkedTokenSource(token.Value)
                :
                new CancellationTokenSource();

            _systemWriter = new StringWriterWithEncoding(Encoding.UTF8);
            var writers = new List<TextWriter>
            {
                _systemWriter,
            };

            if (jobProperties.Output != null)
            {
                writers.Add(jobProperties.Output);
            }

            var multiTextWriter = new MultiTextWriterLab(Encoding.UTF8, writers);

            var dueTimeInfo = _initiator.DueTimeHolder.GetDueTimeInfo();
            var dueTime = dueTimeInfo.GetEffectiveDueTime();
            var dueTimeWasOverridden = dueTimeInfo.IsDueTimeOverridden();

            var now = TimeProvider.GetCurrent();

            _runInfoBuilder = new JobRunInfoBuilder(
                initiator.JobRunsHolder.Count,
                startReason,
                dueTime,
                dueTimeWasOverridden,
                now,
                JobRunStatus.Running,
                _systemWriter);


            // todo: try/catch
            _task = jobProperties.Routine(
                jobProperties.Parameter,
                jobProperties.ProgressTracker,
                multiTextWriter,
                _tokenSource.Token);

            _logger = new ObjectLogger(this, null)
            {
                IsEnabled = true,
            };
        }

        #endregion

        #region Private

        private void EndTask(Task task)
        {
            _logger.Debug($"Task ended. Status: {task.Status}", nameof(EndTask));
            _logger.Debug($"Task ended. Status: {task.Status}", nameof(EndTask), task.Exception?.InnerException);

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

            var now = TimeProvider.GetCurrent();

            _runInfoBuilder.EndTime = now;
            _runInfoBuilder.Status = status;
            _runInfoBuilder.Exception = exception;

            var jobRunInfo = _runInfoBuilder.Build();
            _initiator.JobRunsHolder.Finish(jobRunInfo);

            _tokenSource.Dispose();
            _systemWriter.Dispose();

            _initiator.OnTaskEnded();
        }

        private static Exception ExtractTaskException(AggregateException taskException)
        {
            return taskException?.InnerException ?? taskException;
        }


        #endregion

        #region Internal

        internal void Start()
        {
            _initiator.JobRunsHolder.Start(_runInfoBuilder.Build());
            _task.ContinueWith(this.EndTask);
        }

        internal void Cancel()
        {
            _tokenSource.Cancel(); // todo: throws if disposed. take care of it and ut it.
        }

        #endregion
    }
}
