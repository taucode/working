using TauCode.Extensions;
using TauCode.Working.TestDemo.Common;

namespace TauCode.Working.TestDemo.Client.Executors
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
