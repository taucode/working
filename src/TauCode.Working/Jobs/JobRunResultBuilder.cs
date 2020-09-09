using System;

namespace TauCode.Working.Jobs
{
    internal class JobRunResultBuilder
    {
        internal JobRunResultBuilder(int runIndex, DateTime startedAt)
        {
            this.RunIndex = runIndex;
            this.Start = startedAt;
        }


        internal int RunIndex { get; }
        internal DateTime Start { get; }
        internal DateTime End { get; set; }
        internal JobRunStatus? Status { get; set; }
        internal Exception Exception { get; set; }
        internal string Output { get; set; }

        internal JobRunResult Build()
        {
            var jobRunResult = new JobRunResult(
                this.RunIndex,
                this.Start,
                this.End,
                this.Status ?? throw new NotImplementedException(),
                this.Output,
                this.Exception);

            return jobRunResult;
        }
    }
}
