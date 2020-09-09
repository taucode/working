namespace TauCode.Working.Jobs
{
    public enum JobChangeType
    {
        Registered = 1,
        ParameterChanged = 2,
        ScheduleChanged = 3,
        DueTimeChanged = 4,
        Started = 5,
        Completed = 7,
        Canceled = 8,
        EnabledChanged = 9,
        Removed = 10,
    }
}
