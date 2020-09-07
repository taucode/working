using EasyNetQ;
using System;
using System.Threading.Tasks;
using TauCode.Working.TestDemo.Common.WorkerInterfaces.Queue;
using TauCode.Working.TestDemo.EasyNetQ;

namespace TauCode.Working.TestDemo.Server.Workers
{
    public class SimpleQueueWorker : QueueWorkerBase2<int>, IRabbitWorker
    {
        private readonly IBus _bus;

        public SimpleQueueWorker(IBus bus)
        {
            _bus = bus;
        }

        protected override Task DoAssignmentAsync(int assignment)
        {
            throw new NotImplementedException();
        }

        public IDisposable[] RegisterHandlers()
        {
            var handle = _bus.RespondForWorker<SimpleQueueWorkerRequest, SimpleQueueWorkerResponse>(
                this.Respond,
                this.Name);
            return new[] { handle };
        }

        private SimpleQueueWorkerResponse Respond(SimpleQueueWorkerRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
