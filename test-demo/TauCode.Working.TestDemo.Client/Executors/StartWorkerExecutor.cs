using System;
using System.Collections.Generic;
using System.Linq;
using TauCode.Cli.CommandSummary;
using TauCode.Cli.Data;
using TauCode.Extensions;
using TauCode.Working.TestDemo.Common;

namespace TauCode.Working.TestDemo.Client.Executors
{
    public class StartWorkerExecutor : WorkerExecutorBase
    {
        public StartWorkerExecutor()
            : base(typeof(Program).Assembly.GetResourceText($".{nameof(StartWorkerExecutor)}.lisp", true))
        {
        }

        public override void Process(IList<CliCommandEntry> entries)
        {
            var bus = this.GetBus();

            var request = new InvokeMethodRequest
            {
                MethodName = "Start",
                Arguments = new string[] { },
            };

            var summary = (new CliCommandSummaryBuilder()).Build(this.Descriptor, entries);
            var workerName = summary.Arguments["worker-name"].Single();

            // todo: try/catch
            var response = bus.Request<InvokeMethodRequest, InvokeMethodResponse>(request, conf => conf.WithQueueName(workerName));

            if (response.Exception == null)
            {
                Console.WriteLine($"Worker {workerName} was started.");
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
