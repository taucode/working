using EasyNetQ;
using System;
using System.Threading.Tasks;
using TauCode.Working.TestDemo.Cui.Common;
using TauCode.Working.TestDemo.Cui.Common.WorkerInterfaces.Timeout;
using TauCode.Working.TestDemo.Cui.EasyNetQ;
using TauCode.Working.Workers;

namespace TauCode.Working.TestDemo.Cui.Server.Workers
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
            var handle = _bus.RespondForWorker<SimpleTimeoutWorkerRequest, SimpleTimeoutWorkerResponse>(
                this.Respond,
                this.Name);
            return new[] { handle };
        }

        private SimpleTimeoutWorkerResponse Respond(SimpleTimeoutWorkerRequest request)
        {
            try
            {
                if (request.Timeout.HasValue)
                {
                    this.Timeout = TimeSpan.FromMilliseconds(request.Timeout.Value);
                }

                return new SimpleTimeoutWorkerResponse
                {
                    Timeout = (int)this.Timeout.TotalMilliseconds,
                };
            }
            catch (Exception ex)
            {
                return new SimpleTimeoutWorkerResponse
                {
                    Exception = ExceptionInfo.FromException(ex),
                };
            }
        }
    }
}
