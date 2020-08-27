using TauCode.Extensions;
using TauCode.Working.TestDemo.Common;

namespace TauCode.Working.TestDemo.Client.Executors
{
    public class StopWorkerExecutor : CommandWorkerExecutorBase
    {
        public StopWorkerExecutor()
            : base(
                typeof(Program).Assembly.GetResourceText($".{nameof(StopWorkerExecutor)}.lisp", true),
                WorkerCommand.Stop)
        {
        }
    }
}
