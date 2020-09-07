using EasyNetQ;
using System;
using System.Threading.Tasks;
using TauCode.Working.TestDemo.Common;
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

        protected override async Task DoAssignmentAsync(int assignment)
        {
            Console.WriteLine($"Assignment: {assignment}");
            Console.WriteLine($"Will delay for: {this.JobDelay} ms.");

            await Task.Delay(this.JobDelay);
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
            try
            {
                if (request.JobDelay.HasValue)
                {
                    this.JobDelay = request.JobDelay.Value;
                }

                if (request.From.HasValue)
                {
                    for (var i = request.From.Value; i <= request.To.Value; i++)
                    {
                        this.Enqueue(i);
                    }
                }

                return new SimpleQueueWorkerResponse
                {
                    Backlog = this.Backlog,
                    JobDelay = this.JobDelay,
                };
            }
            catch (Exception ex)
            {
                return new SimpleQueueWorkerResponse
                {
                    Exception = ExceptionInfo.FromException(ex),
                };
            }

        }

        public int JobDelay { get; set; }
    }
}
