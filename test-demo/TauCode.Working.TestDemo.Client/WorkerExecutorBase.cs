using EasyNetQ;
using System;
using TauCode.Cli;
using TauCode.Working.TestDemo.Common;

namespace TauCode.Working.TestDemo.Client
{
    public abstract class WorkerExecutorBase : CliExecutorBase
    {
        protected WorkerExecutorBase(
            string grammar)
            : base(grammar, "1.0", true)
        {
        }

        protected IBus GetBus() => ((WorkerHost)this.AddIn.Host).Bus;

        protected void ShowResult(string result, ExceptionInfo exception)
        {
            if (exception == null)
            {
                Console.WriteLine($"Result from server: '{result}'");
            }

            if (exception != null)
            {
                Console.WriteLine("Server returned exception:");
                Console.WriteLine(exception.TypeName);
                Console.WriteLine(exception.Message);
            }
        }
    }
}
