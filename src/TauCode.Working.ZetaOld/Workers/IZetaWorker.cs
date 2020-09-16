using System;

namespace TauCode.Working.ZetaOld.Workers
{
    public interface IZetaWorker : IDisposable
    {
        string Name { get; set; }
        ZetaWorkerState State { get; }
        void Start();
        void Pause();
        void Resume();
        void Stop();
        ZetaWorkerState? WaitForStateChange(int millisecondsTimeout, params ZetaWorkerState[] states);
    }
}
