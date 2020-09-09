using TauCode.Extensions;
using TauCode.Working.TestDemo.Cui.Common;

namespace TauCode.Working.TestDemo.Cui.Client.Executors
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
