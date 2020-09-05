using System;

namespace TauCode.Working.TestDemo.Server
{
    public interface IRabbitWorker : IWorker
    {
        IDisposable[] RegisterHandlers();
    }
}
