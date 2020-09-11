using System;
using System.Collections.Generic;
using System.Linq;

namespace TauCode.Working.Jobs
{
    public readonly struct JobInfo
    {
        internal JobInfo(
            string name,
            JobRunInfo? currentRun,
            DueTimeInfo dueTimeInfo,
            int runCount,
            IEnumerable<JobRunInfo> runs)
        {
            if (runs == null)
            {
                throw new ArgumentNullException(nameof(runs));
            }

            this.Name = name;
            this.CurrentRun = currentRun;
            this.DueTimeInfo = dueTimeInfo;
            this.RunCount = runCount;
            this.Runs = runs.ToList();
        }

        public string Name { get; }
        public JobRunInfo? CurrentRun { get; }
        public DueTimeInfo DueTimeInfo { get; }
        public int RunCount { get; }
        public IReadOnlyList<JobRunInfo> Runs { get; }
    }
}
