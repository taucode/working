using System;
using TauCode.Working.ZetaOld.Workers;

namespace TauCode.Working.TestDemo.Cui.Server
{
    public interface IRabbitWorker : IZetaWorker
    {
        IDisposable[] RegisterHandlers();
    }
}
