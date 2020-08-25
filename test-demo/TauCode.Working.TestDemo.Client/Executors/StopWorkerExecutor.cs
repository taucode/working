using System;
using System.Collections.Generic;
using System.Linq;
using TauCode.Cli.CommandSummary;
using TauCode.Cli.Data;
using TauCode.Extensions;
using TauCode.Working.TestDemo.Common;

namespace TauCode.Working.TestDemo.Client.Executors
{
    public class StopWorkerExecutor : WorkerExecutorBase
    {
        public StopWorkerExecutor()
            : base(typeof(Program).Assembly.GetResourceText($".{nameof(StopWorkerExecutor)}.lisp", true))
        {
        }

        public override void Process(IList<CliCommandEntry> entries)
        {
            var bus = this.GetBus();

            var request = new InvokeMethodRequest
            {
                MethodName = "Stop",
                Arguments = new string[] { },
            };

            var summary = (new CliCommandSummaryBuilder()).Build(this.Descriptor, entries);
            var workerName = summary.Arguments["worker-name"].Single();

            // todo: try/catch
            var response = bus.Request<InvokeMethodRequest, InvokeMethodResponse>(request, conf => conf.WithQueueName(workerName));

            if (response.Exception == null)
            {
                Console.WriteLine($"Worker {workerName} was stopped.");
            }
            else
            {
                Console.WriteLine("Server returned exception:");
                Console.WriteLine(response.Exception.TypeName);
                Console.WriteLine(response.Exception.Message);
            }
        }
    }
}
