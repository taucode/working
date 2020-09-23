using System.Collections.Generic;
using System.Linq;
using System.Text;
using TauCode.Cli.CommandSummary;
using TauCode.Cli.Data;
using TauCode.Extensions;
using TauCode.Working.TestDemo.Cui.Common.WorkerInterfaces.Queue;
using TauCode.Working.TestDemo.Cui.EasyNetQ;

namespace TauCode.Working.TestDemo.Cui.Client.Executors
{
    public class QueueWorkerExecutor : WorkerExecutorBase
    {
        public QueueWorkerExecutor()
            : base(
                typeof(Program).Assembly.GetResourceText($".{nameof(QueueWorkerExecutor)}.lisp", true))
        {
        }

        public override void Process(IList<CliCommandEntry> entries)
        {
            var summary = (new CliCommandSummaryBuilder()).Build(this.Descriptor, entries);
            var workerName = summary.Arguments["worker-name"].Single();

            var from = this.ConvertInputArgument(summary.Arguments["queue-from"].Single());
            var to = this.ConvertInputArgument(summary.Arguments["queue-to"].Single());

            var jobDelay = summary.Arguments["job-delay"].SingleOrDefault()?.ToInt32();

            var request = new SimpleQueueWorkerRequest
            {
                From = from,
                To = to,
                JobDelay = jobDelay,
            };


            var response = this.GetBus().RequestForWorker<SimpleQueueWorkerRequest, SimpleQueueWorkerResponse>(
                request,
                workerName);

            var resultSb = new StringBuilder();

            resultSb.AppendLine($"Backlog: {response.Backlog};");
            resultSb.AppendLine($"JobDelay: {response.JobDelay};");

            this.ShowResult(resultSb.ToString(), response.Exception);
        }

        private int? ConvertInputArgument(string inputValue)
        {
            if (inputValue == "-1")
            {
                return null;
            }

            return inputValue.ToInt32();
        }
    }
}
