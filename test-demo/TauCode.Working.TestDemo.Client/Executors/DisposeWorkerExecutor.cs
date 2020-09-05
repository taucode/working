using TauCode.Extensions;
using TauCode.Working.TestDemo.Common;

namespace TauCode.Working.TestDemo.Client.Executors
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
