using System;
using Microsoft.Extensions.Logging;

namespace TauCode.Working
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
        ILogger Logger { get; set; }
    }
}
