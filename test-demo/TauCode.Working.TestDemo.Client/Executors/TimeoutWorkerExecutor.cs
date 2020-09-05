using System.Collections.Generic;
using TauCode.Cli.Data;
using TauCode.Extensions;

namespace TauCode.Working.TestDemo.Client.Executors
{
    public class TimeoutWorkerExecutor : WorkerExecutorBase
    {
        public TimeoutWorkerExecutor()
            : base(
                typeof(Program).Assembly.GetResourceText($".{nameof(TimeoutWorkerExecutor)}.lisp", true))
        {
        }

        public override void Process(IList<CliCommandEntry> entries)
        {
            throw new System.NotImplementedException();
        }
    }
}
