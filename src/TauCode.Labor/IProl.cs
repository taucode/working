using System;

namespace TauCode.Labor
{
    public interface IProl : IDisposable
    {
        ProlState State { get; }
        void Start();
        void Stop();
        bool IsDisposed { get; }
    }
}
