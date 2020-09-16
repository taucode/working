namespace TauCode.Working.Jobs
{
    public enum JobRunStatus
    {
        Unknown = 0,
        FailedToStart = 1,
        Running = 2,
        Succeeded = 3,
        Faulted = 4,
        Canceled = 5,
    }
}
