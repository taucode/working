using System;

namespace TauCode.Working.Jobs
{
    public readonly struct JobRunInfo
    {
        public JobRunInfo(
            int index,
            StartReason startReason,
            DueTimeInfo dueTimeInfo,
            DateTime startTime,
            DateTime? endTime,
            JobRunStatus status,
            string output,
            Exception exception)
        {
            // todo checks: positive etc
            // todo date is valid

            this.Index = index;
            this.StartReason = startReason;
            this.DueTimeInfo = dueTimeInfo;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.Status = status;
            this.Output = output;
            this.Exception = exception;
        }

        public int Index { get; }
        public StartReason StartReason { get; }
        public DueTimeInfo DueTimeInfo { get; }
        public DateTime StartTime { get; }
        public DateTime? EndTime { get; }
        public JobRunStatus Status { get; }
        public string Output { get; }
        public Exception Exception { get; }
    }
}
