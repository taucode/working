﻿using System;

namespace TauCode.Working.Jobs
{
    internal class JobRunInfoBuilder
    {
        internal JobRunInfoBuilder(
            int runIndex,
            StartReason startReason,
            DueTimeInfo dueTimeInfo,
            DateTime startTime)
        {
            this.RunIndex = runIndex;
            this.StartReason = startReason;
            this.DueTimeInfo = dueTimeInfo;
            this.StartTime = startTime;
        }


        internal int RunIndex { get; }
        internal StartReason StartReason { get; }
        internal DueTimeInfo DueTimeInfo { get; }
        internal DateTime StartTime { get; }
        internal DateTime? EndTime { get; set; }
        internal JobRunStatus? Status { get; set; }
        internal Exception Exception { get; set; }
        internal string Output { get; set; }

        internal JobRunInfo Build()
        {
            var jobRunResult = new JobRunInfo(
                this.RunIndex,
                this.StartReason,
                this.DueTimeInfo,
                this.StartTime,
                this.EndTime,
                this.Status ?? throw new NotImplementedException(),
                this.Output,
                this.Exception);

            return jobRunResult;
        }
    }
}