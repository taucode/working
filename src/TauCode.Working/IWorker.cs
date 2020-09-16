using System;

namespace TauCode.Working
{
    public interface IWorker : IDisposable
    {
        string Name { get; }
        WorkerState State { get; }
        void Start();
        void Stop();
        bool IsDisposed { get; }
    }
}
