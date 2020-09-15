using System;

namespace TauCode.Working.Jobs
{
    public readonly struct JobRunInfo
    {
        public JobRunInfo(
            int runIndex,
            JobStartReason startReason,
            DateTimeOffset dueTime,
            bool dueTimeWasOverridden,
            DateTimeOffset startTime,
            DateTimeOffset? endTime,
            JobRunStatus status,
            string output,
            Exception exception)
        {
            // todo checks: positive etc
            // todo date is valid

            this.RunIndex = runIndex;
            this.StartReason = startReason;
            this.DueTime = dueTime;
            this.DueTimeWasOverridden = dueTimeWasOverridden;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.Status = status;
            this.Output = output;
            this.Exception = exception;
        }

        public int RunIndex { get; }
        public JobStartReason StartReason { get; }
        public DateTimeOffset DueTime { get; }
        public bool DueTimeWasOverridden { get; }
        public DateTimeOffset StartTime { get; }
        public DateTimeOffset? EndTime { get; }
        public JobRunStatus Status { get; }
        public string Output { get; }
        public Exception Exception { get; }
    }
}
