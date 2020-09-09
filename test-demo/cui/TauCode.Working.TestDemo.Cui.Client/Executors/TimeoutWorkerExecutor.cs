using System.Collections.Generic;
using System.Linq;
using TauCode.Cli.CommandSummary;
using TauCode.Cli.Data;
using TauCode.Extensions;
using TauCode.Working.TestDemo.Cui.Common.WorkerInterfaces.Timeout;
using TauCode.Working.TestDemo.Cui.EasyNetQ;

namespace TauCode.Working.TestDemo.Cui.Client.Executors
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
            var summary = (new CliCommandSummaryBuilder()).Build(this.Descriptor, entries);
            var workerName = summary.Arguments["worker-name"].Single();
            int? timeout = summary.Arguments["timeout-value"].SingleOrDefault()?.ToInt32();

            var request = new SimpleTimeoutWorkerRequest
            {
                Timeout = timeout,
            };

            var response = this.GetBus().RequestForWorker<SimpleTimeoutWorkerRequest, SimpleTimeoutWorkerResponse>(
                request,
                workerName);

            this.ShowResult(response.Timeout.ToString(), response.Exception);
        }
    }
}
