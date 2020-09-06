using System.Collections.Generic;

namespace TauCode.Working.Schedules
{
    public interface IScheduleManager
    {
        string RegisterWorker(IScheduledWorker scheduledWorker);
        void UnregisterWorker(IScheduledWorker scheduledWorker);
        void UnregisterWorker(string registrationId);
        IReadOnlyDictionary<string, IScheduledWorker> GetWorkers();
        void EnableSchedule(string registrationId);
        void DisableSchedule(string registrationId);
    }
}
