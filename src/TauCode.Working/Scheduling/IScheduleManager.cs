using System;

namespace TauCode.Working.Scheduling
{
    public interface IScheduleManager : IDisposable
    {
        void Start();

        string Register(AutoStopWorkerBase worker, ISchedule schedule);

        void ChangeSchedule(string registrationId, ISchedule schedule);

        void Remove(string registrationId);

        //string RegisterWorker(IScheduledWorker scheduledWorker);
        //void UnregisterWorker(IScheduledWorker scheduledWorker);
        //void UnregisterWorker(string registrationId);
        //IReadOnlyDictionary<string, ScheduledWorkerRegistration> GetWorkers();

        void Enable(string registrationId);

        void Disable(string registrationId);
    }
}
