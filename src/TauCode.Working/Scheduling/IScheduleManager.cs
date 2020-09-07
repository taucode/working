using System;
using System.Collections.Generic;

namespace TauCode.Working.Scheduling
{
    public interface IScheduleManager : IDisposable
    {
        void Start();
        string RegisterWorker(IScheduledWorker scheduledWorker);
        void UnregisterWorker(IScheduledWorker scheduledWorker);
        void UnregisterWorker(string registrationId);
        IReadOnlyDictionary<string, ScheduledWorkerRegistration> GetWorkers();
        void EnableSchedule(string registrationId);
        void DisableSchedule(string registrationId);
    }
}
