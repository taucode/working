using EasyNetQ;
using TauCode.Cli;

namespace TauCode.Working.TestDemo.Client
{
    public abstract class WorkerExecutorBase : CliExecutorBase
    {
        protected WorkerExecutorBase(
            string grammar)
            : base(grammar, "1.0", true)
        {
        }

        protected IBus GetBus() => ((WorkerHost)this.AddIn.Host).Bus;
    }
}
