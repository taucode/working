using System;
using System.Threading.Tasks;

namespace TauCode.Working.TestDemo.Server.Workers
{
    public class PersonTimeoutWorker : TimeoutWorkerBase
    {
        public const string InitialTimeoutString = "00:00:01";

        public PersonTimeoutWorker()
            : base(TimeSpan.Parse(InitialTimeoutString))
        {
        }

        protected override Task DoRealWorkAsync()
        {
            throw new NotImplementedException();
        }
    }
}
