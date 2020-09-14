namespace TauCode.Working.Jobs
{
    internal enum JobStartResult
    {
        Unknown2 = 0,
        AlreadyStartedByScheduleDueTime = 1,
        AlreadyStartedByOverriddenDueTime = 2,
        AlreadyStartedByForce2 = 3,
        Started2 =  3,
        AlreadyDisposed2 = 4,
    }
}
