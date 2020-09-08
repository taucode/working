using System;
using TauCode.Extensions.Lab;

namespace TauCode.Working.Jobs
{
    public class JobRunResult
    {
        public int Index { get; }
        public DateTimeInterval Interval { get; }
        public JobRunStatus Status { get; }
        public string Output { get; }
        public Exception Exception { get; }
    }
}
