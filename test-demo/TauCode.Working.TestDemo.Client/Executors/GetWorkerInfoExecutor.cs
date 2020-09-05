using TauCode.Extensions;
using TauCode.Working.TestDemo.Common;

namespace TauCode.Working.TestDemo.Client.Executors
{
    public class GetWorkerInfoExecutor : CommandWorkerExecutorBase
    {
        public GetWorkerInfoExecutor()
            : base(
                typeof(Program).Assembly.GetResourceText($".{nameof(GetWorkerInfoExecutor)}.lisp", true),
                WorkerCommand.GetInfo)
        {
        }
    }
}
