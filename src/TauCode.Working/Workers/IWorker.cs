using System;

namespace TauCode.Working.Workers
{
    public interface IWorker : IDisposable
    {
        string Name { get; set; }
        WorkerState State { get; }
        void Start();
        void Pause();
        void Resume();
        void Stop();
        WorkerState? WaitForStateChange(int millisecondsTimeout, params WorkerState[] states);
    }
}
