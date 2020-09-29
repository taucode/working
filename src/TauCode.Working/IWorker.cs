using System;

namespace TauCode.Working
{
    public interface IWorker : IDisposable
    {
        string Name { get; set; }
        WorkerState State { get; }
        void Start();
        void Stop();
        bool IsDisposed { get; }
    }
}
