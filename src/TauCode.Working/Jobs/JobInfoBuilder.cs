namespace TauCode.Working.Jobs
{
    internal class JobInfoBuilder
    {
        internal JobInfoBuilder(string name)
        {
            this.Name = name;
        }

        internal string Name { get; }
        internal DueTimeInfo DueTimeInfo { get; set; }
        internal bool IsEnabled { get; set; }

        internal JobInfo Build()
        {
            return new JobInfo(
                this.Name,
                null,
                this.DueTimeInfo,
                this.IsEnabled,
                0,
                new JobRunInfo[0]);
        }
    }
}
