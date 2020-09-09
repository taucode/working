using TauCode.Extensions;
using TauCode.Working.TestDemo.Cui.Common;

namespace TauCode.Working.TestDemo.Cui.Client.Executors
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
