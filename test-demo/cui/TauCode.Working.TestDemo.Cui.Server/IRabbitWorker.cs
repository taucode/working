using System;
using TauCode.Working.Workers;

namespace TauCode.Working.TestDemo.Cui.Server
{
    public interface IRabbitWorker : IWorker
    {
        IDisposable[] RegisterHandlers();
    }
}
