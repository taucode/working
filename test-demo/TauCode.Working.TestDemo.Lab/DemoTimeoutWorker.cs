using System;
using System.Threading.Tasks;
using TauCode.Working.Workers;

namespace TauCode.Working.TestDemo.Lab
{
    public class DemoTimeoutWorker : TimeoutWorkerBase
    {
        private int _step;

        public const string InitialTimeoutString = "00:00:01";

        public DemoTimeoutWorker()
            : base(TimeSpan.Parse(InitialTimeoutString))
        {
            _step = 0;
        }

        protected override Task DoRealWorkAsync()
        {
            Console.WriteLine($"Step {_step}");
            _step++;
            return Task.CompletedTask;
        }
    }
}
