using Serilog;
using System.Threading;
using System.Threading.Tasks;

namespace TauCode.Working.Lab.Tests.Server
{
    public class FooWorker : QueueWorkerBase<string>
    {
        public FooWorker()
        {
        }

        protected override void DoAssignment(string assignment)
        {
            //if (assignment.EndsWith("10"))
            //{
            //    throw new AbandonedMutexException("ha ha ha");
            //}

            //Log.Information($"Performing '{assignment}'");

            //var timeout = 200;
            //Log.Information($"Waiting {timeout} ms.");
            //Task.Delay(timeout).Wait();
        }
    }
}
