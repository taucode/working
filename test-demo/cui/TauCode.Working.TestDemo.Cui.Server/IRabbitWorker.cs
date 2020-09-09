using System;

namespace TauCode.Working.TestDemo.Cui.Server
{
    public interface IRabbitWorker : IWorker
    {
        IDisposable[] RegisterHandlers();
    }
}
