using System;

namespace TauCode.Working.Labor
{
    public interface ILaborer : IDisposable
    {
        string Name { get; set; }
        LaborerState State { get; }
        void Start();
        void Stop();
        void Pause();
        void Resume();
        bool IsDisposed { get; }
    }
}
