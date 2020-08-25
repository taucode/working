using Serilog;
using System;
using System.Threading.Tasks;

namespace TauCode.Working.TestDemo.Lab
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();

            var worker = new DemoTimeoutWorker();
            worker.Start();

            Console.ReadLine();

            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(100);
                worker.Dispose();
            });

            var workerState = worker.WaitForStateChange(1200, WorkerState.Disposed);
        }
    }
}
