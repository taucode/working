namespace TauCode.Working.Schedules
{
    public class ScheduledWorkerRegistration
    {
        public string RegistrationId { get; }
        public IScheduledWorker Worker { get; }
        public ScheduleState State { get; }
    }
}
