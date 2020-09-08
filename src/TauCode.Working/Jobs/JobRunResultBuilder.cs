using System;

namespace TauCode.Working.Jobs
{
    internal class JobRunResultBuilder
    {
        internal JobRunResultBuilder(int runIndex, DateTime startedAt)
        {
            this.RunIndex = runIndex;
            this.StartedAt = startedAt;
        }


        internal int RunIndex { get; }
        internal DateTime StartedAt { get; }
        internal DateTime FinishedAt { get; set; }
        internal JobRunStatus? Status { get; set; }
        internal Exception Exception { get; set; }
        internal string Output { get; set; }
    }
}
