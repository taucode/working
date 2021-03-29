﻿using Microsoft.Extensions.Logging;
using System;

namespace TauCode.Working
{
    public interface IWorker : IDisposable
    {
        string Name { get; set; }
        WorkerState State { get; }
        void Start();
        void Stop();
        void Pause();
        void Resume();
        bool IsDisposed { get; }
        ILogger Logger { get; set; }
    }
}
