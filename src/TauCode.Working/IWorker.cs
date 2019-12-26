using System;

namespace TauCode.Working.Lab
{
    public interface IWorker : IDisposable
    {
        string Name { get; set; }
        WorkerState State { get; }
        void Start();
        void Pause();
        void Resume();
        void Stop();
    }
}
