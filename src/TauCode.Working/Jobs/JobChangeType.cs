namespace TauCode.Working.Jobs
{
    public enum JobChangeType
    {
        Registered = 1,
        ParameterChanged,
        ScheduleChanged,
        DueTimeChanged,
        DueTimeReset,
        ForceStarted,
        Canceled,
        IsEnabledChanged,
        Removed,
    }
}
