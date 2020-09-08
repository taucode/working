using System;

namespace TauCode.Working.Jobs
{
    public class JobInfo
    {
        public string Name { get; }
        public DateTime? NextDueTime { get; }
        public bool IsEnabled { get; }
        public DateTime? LastStartTime { get; }
        public JobRunResult[] Log { get; }
    }
}
