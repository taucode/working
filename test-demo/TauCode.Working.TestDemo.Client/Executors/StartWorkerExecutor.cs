using TauCode.Extensions;
using TauCode.Working.TestDemo.Common;

namespace TauCode.Working.TestDemo.Client.Executors
{
    public class StartWorkerExecutor : CommandWorkerExecutorBase
    {
        public StartWorkerExecutor()
            : base(
                typeof(Program).Assembly.GetResourceText($".{nameof(StartWorkerExecutor)}.lisp", true),
                WorkerCommand.Start)
        {
        }
    }
}
