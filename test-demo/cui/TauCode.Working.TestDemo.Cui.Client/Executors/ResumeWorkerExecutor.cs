using TauCode.Extensions;
using TauCode.Working.TestDemo.Cui.Common;

namespace TauCode.Working.TestDemo.Cui.Client.Executors
{
    public class ResumeWorkerExecutor : CommandWorkerExecutorBase
    {
        public ResumeWorkerExecutor()
            : base(
                typeof(Program).Assembly.GetResourceText($".{nameof(ResumeWorkerExecutor)}.lisp", true),
                WorkerCommand.Resume)
        {
        }
    }
}
