using System.Collections.Generic;

namespace TauCode.Working.Jobs
{
    // todo: readonly struct?
    public class JobInfo
    {
        public string Name { get; }
        public JobRunInfo? CuurentRun { get; }
        public DueTimeInfo DueTimeInfo { get; }
        public bool IsEnabled { get; }
        public int RunCount { get; set; }
        public IList<JobRunInfo> Runs { get; }
    }
}
