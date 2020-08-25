using System.Collections.Generic;
using TauCode.Cli;
using TauCode.Cli.Data;
using TauCode.Extensions;

namespace TauCode.Working.TestDemo.Client.Executors
{
    public class StartWorkerExecutor : CliExecutorBase
    {
        public StartWorkerExecutor()
            : base(
                typeof(Program).Assembly.GetResourceText($".{nameof(StartWorkerExecutor)}.lisp", true),
                "1.0",
                true)
        {
        }

        public override void Process(IList<CliCommandEntry> entries)
        {
            throw new System.NotImplementedException();
        }
    }
}
