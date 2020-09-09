using System;

namespace TauCode.Working.Jobs
{
    public class JobChangedEventArgs : EventArgs
    {
        internal JobChangedEventArgs(string jobName, JobChangeType changeType)
        {
            this.JobName = jobName ?? throw new ArgumentNullException(nameof(jobName));
            this.ChangeType = changeType;
        }

        public string JobName { get; }

        public JobChangeType ChangeType { get; }

        public bool? IsEnabled { get; }

        public DueTimeInfo DueTimeInfo { get; }

        public bool? ManuallyStarted { get; }
    }
}
