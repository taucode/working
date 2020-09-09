using System;

namespace TauCode.Working.Jobs
{
    public class JobChangedEventArgs : EventArgs
    {
        public JobChangedEventArgs(string jobName, JobChangeType changeType)
        {
            this.JobName = jobName ?? throw new ArgumentNullException(nameof(jobName));
            this.ChangeType = changeType;
        }

        public string JobName { get; }

        public JobChangeType ChangeType { get; }
    }
}
