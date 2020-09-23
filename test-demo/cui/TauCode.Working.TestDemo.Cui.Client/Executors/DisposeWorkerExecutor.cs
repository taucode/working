using TauCode.Extensions;
using TauCode.Working.TestDemo.Cui.Common;

namespace TauCode.Working.TestDemo.Cui.Client.Executors
{
    public class DisposeWorkerExecutor : CommandWorkerExecutorBase
    {
        public DisposeWorkerExecutor()
            : base(
                typeof(Program).Assembly.GetResourceText($".{nameof(DisposeWorkerExecutor)}.lisp", true),
                WorkerCommand.Dispose)
        {
        }
    }
}
