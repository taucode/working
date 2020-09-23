using System;
using EasyNetQ;
using TauCode.Cli;
using TauCode.Working.TestDemo.Cui.Common;

namespace TauCode.Working.TestDemo.Cui.Client
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
                Console.WriteLine(result);
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
