using System;

namespace TauCode.Labor
{
    public interface IProl : IDisposable
    {
        string Name { get; }
        ProlState State { get; }
        void Start();
        void Stop();
        bool IsDisposed { get; }
    }
}
