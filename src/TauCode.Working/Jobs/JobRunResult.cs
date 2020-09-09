using System;
using TauCode.Extensions.Lab;

namespace TauCode.Working.Jobs
{
    public readonly struct JobRunResult
    {
        public JobRunResult(
            int index,
            DateTime start,
            DateTime end,
            JobRunStatus status,
            string output,
            Exception exception)
        {
            // todo checks: positive etc

            this.Index = index;
            this.Interval = new DateTimeInterval(start, end);
            this.Status = status;
            this.Output = output;
            this.Exception = exception;
        }

        public int Index { get; }
        public DateTimeInterval Interval { get; }
        public JobRunStatus Status { get; }
        public string Output { get; }
        public Exception Exception { get; }
    }
}
