namespace TauCode.Working.Jobs
{
    public enum JobChangeType
    {
        Registered = 1,
        ParameterChanged = 2,
        ScheduleChanged = 3,
        DueTimeChanged = 4,
        ForceStarted = 5,
        Canceled = 6,
        IsEnabledChanged = 7,
        Removed = 8,
    }
}
