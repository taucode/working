using System.Collections.Generic;
using System.Linq;
using TauCode.Cli.CommandSummary;
using TauCode.Cli.Data;
using TauCode.Extensions;
using TauCode.Working.TestDemo.Common;

namespace TauCode.Working.TestDemo.Client.Executors
{
    // todo clean up
    public class ResumeWorkerExecutor : WorkerExecutorBase
    {
        public ResumeWorkerExecutor()
            : base(typeof(Program).Assembly.GetResourceText($".{nameof(ResumeWorkerExecutor)}.lisp", true))
        {
        }

        public override void Process(IList<CliCommandEntry> entries)
        {
            var bus = this.GetBus();

            var request = new WorkerCommandRequest
            {
                Command = WorkerCommand.Resume,
            };

            var summary = (new CliCommandSummaryBuilder()).Build(this.Descriptor, entries);
            var workerName = summary.Arguments["worker-name"].Single();

            // todo: try/catch
            var response = bus.Request<WorkerCommandRequest, WorkerCommandResponse>(
                request,
                conf => conf.WithQueueName(workerName));

            this.ShowResult(response.Result, response.Exception);
        }
    }
}
