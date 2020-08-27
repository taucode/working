using TauCode.Extensions;
using TauCode.Working.TestDemo.Common;

namespace TauCode.Working.TestDemo.Client.Executors
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
