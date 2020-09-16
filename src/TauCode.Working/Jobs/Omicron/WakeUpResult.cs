namespace TauCode.Working.Jobs.Omicron
{
    internal enum WakeUpResult
    {
        Unknown = 0,

        NotRunningAndNotEnabled = 1, // todo ut

        AlreadyDisposed = 2,
        AlreadyRunning = 3,

        StartedBySchedule = 4,
        StartedByOverriddenDueTime = 5,
        StartedByForce = 6,
    }
}
