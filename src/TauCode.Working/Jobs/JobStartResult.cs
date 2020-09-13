namespace TauCode.Working.Jobs
{
    internal enum JobStartResult
    {
        Unknown = 0,
        AlreadyStartedByDueTime = 1,
        AlreadyStartedByForce = 2,
        Started =  3,
        AlreadyDisposed = 4,
    }
}
