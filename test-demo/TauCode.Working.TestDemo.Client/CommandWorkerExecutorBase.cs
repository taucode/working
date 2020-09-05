using System.Collections.Generic;
using System.Linq;
using TauCode.Cli.CommandSummary;
using TauCode.Cli.Data;
using TauCode.Working.TestDemo.Common;

namespace TauCode.Working.TestDemo.Client
{
    public class CommandWorkerExecutorBase : WorkerExecutorBase
    {
        public CommandWorkerExecutorBase(string grammar, WorkerCommand command)
            : base(grammar)
        {
            this.Command = command;
        }

        public WorkerCommand Command { get; }

        public override void Process(IList<CliCommandEntry> entries)
        {
            var bus = this.GetBus();

            var request = new WorkerCommandRequest
            {
                Command = this.Command,
            };

            var summary = (new CliCommandSummaryBuilder()).Build(this.Descriptor, entries);
            var workerName = summary.Arguments["worker-name"].Single();

            var response = bus.Request<WorkerCommandRequest, WorkerCommandResponse>(
                request,
                conf => conf.WithQueueName(workerName));

            this.ShowResult(response.Result, response.Exception);
        }
    }
}
