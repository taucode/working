using TauCode.Extensions;
using TauCode.Working.TestDemo.Cui.Common;

namespace TauCode.Working.TestDemo.Cui.Client.Executors
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
