using TauCode.Extensions;
using TauCode.Working.TestDemo.Cui.Common;

namespace TauCode.Working.TestDemo.Cui.Client.Executors
{
    public class PauseWorkerExecutor : CommandWorkerExecutorBase
    {
        public PauseWorkerExecutor()
            : base(
                typeof(Program).Assembly.GetResourceText($".{nameof(PauseWorkerExecutor)}.lisp", true),
                WorkerCommand.Pause)
        {
        }
    }
}
