using EasyNetQ;
using System;
using System.Threading.Tasks;
using TauCode.Working.TestDemo.Common.WorkerInterfaces.Timeout;

namespace TauCode.Working.TestDemo.Server.Workers
{
    public class SimpleTimeoutWorker : TimeoutWorkerBase, IRabbitWorker
    {
        public const string InitialTimeoutString = "00:00:05";

        private readonly IBus _bus;
        private int _index;

        public SimpleTimeoutWorker(IBus bus)
            : base(TimeSpan.Parse(InitialTimeoutString))
        {
            _bus = bus;
        }

        protected override Task DoRealWorkAsync()
        {
            Console.WriteLine($"My name is {this.Name}. Index: {_index}.");
            _index++;
            return Task.CompletedTask;
        }

        public IDisposable[] RegisterHandlers()
        {
            var rpc = _bus.Respond<SimpleTimeoutWorkerRequest, SimpleTimeoutWorkerResponse>(this.Respond);
            return new[] { rpc };
        }

        private SimpleTimeoutWorkerResponse Respond(
            SimpleTimeoutWorkerRequest request)
        {
            throw new NotImplementedException();
        }
    }
}