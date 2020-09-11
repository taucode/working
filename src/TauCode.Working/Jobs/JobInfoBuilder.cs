using System.Collections.Generic;

namespace TauCode.Working.Jobs
{
    internal class JobInfoBuilder
    {
        internal JobInfoBuilder(string name)
        {
            this.Name = name;
            this.Runs = new List<JobRunInfo>();
        }

        internal string Name { get; }
        internal int RunCount { get; set; }
        internal DueTimeInfo DueTimeInfo { get; set; }
        internal IList<JobRunInfo> Runs { get; }

        internal JobInfo Build()
        {
            return new JobInfo(
                this.Name,
                null,
                this.DueTimeInfo,
                this.RunCount,
                this.Runs);
        }
    }
}
