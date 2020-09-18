using System;

namespace TauCode.Working.Jobs
{
    public static class JobLoggingHelper
    {
        public static void EnableLogging(IJobManager jobManager, bool enable)
        {
            if (jobManager == null)
            {
                throw new ArgumentNullException(nameof(jobManager));
            }

            if (jobManager is JobManager jobManagerInstance)
            {
                jobManagerInstance.IsLoggingEnabled = enable;
            }
            else
            {
                throw new ArgumentException($"'{nameof(jobManager)}' is not of type '{typeof(JobManager).FullName}'.");
            }
        }
    }
}
