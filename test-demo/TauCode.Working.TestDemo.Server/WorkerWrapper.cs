using EasyNetQ;
using System;
using System.Text;
using System.Threading.Tasks;
using TauCode.Working.TestDemo.Common;

// todo clean up
namespace TauCode.Working.TestDemo.Server
{
    public class WorkerWrapper
    {
        private readonly IRabbitWorker _worker;
        //private readonly string _connectionString;

        private readonly IBus _bus;

        public WorkerWrapper(IRabbitWorker worker, IBus bus)
        {
            _worker = worker;
            _bus = bus;
        }

        public async Task Run()
        {
            var rpcHandle1 = _bus.Respond<WorkerCommandRequest, WorkerCommandResponse>(
                this.ProcessMethodInvocation,
                configuration => configuration.WithQueueName(_worker.Name));

            var workerRpcHandles = _worker.RegisterHandlers();

            Console.WriteLine($"{_worker.GetType().FullName} '{_worker.Name}' is ready to work.");

            _worker.WaitForStateChange(System.Threading.Timeout.Infinite, WorkerState.Disposed);

            // wait a bit. let RabbitMQ send farewell response.
            Console.WriteLine("Worker disposed. Waiting 100 ms and exiting Run routine.");
            await Task.Delay(100);

            rpcHandle1.Dispose();

            foreach (var handle in workerRpcHandles)
            {
                handle.Dispose();
            }
        }

        private WorkerCommandResponse ProcessMethodInvocation(WorkerCommandRequest request)
        {
            try
            {
                var result = this.ExecuteCommand(request.Command);
                var response = new WorkerCommandResponse
                {
                    Result = result,
                };

                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = new WorkerCommandResponse
                {
                    Exception = ExceptionInfo.FromException(ex),
                };

                return errorResponse;
            }
        }

        private string ExecuteCommand(WorkerCommand command)
        {
            string result;

            switch (command)
            {
                case WorkerCommand.GetInfo:
                    result = this.GetInfo();
                    break;

                case WorkerCommand.Start:
                    _worker.Start();
                    result = _worker.State.ToString();
                    break;

                case WorkerCommand.Pause:
                    _worker.Pause();
                    result = _worker.State.ToString();
                    break;

                case WorkerCommand.Resume:
                    _worker.Resume();
                    result = _worker.State.ToString();
                    break;

                case WorkerCommand.Stop:
                    _worker.Stop();
                    result = _worker.State.ToString();
                    break;

                case WorkerCommand.Dispose:
                    _worker.Dispose();
                    result = _worker.State.ToString();
                    break;

                default:
                    throw new NotImplementedException(); // todo
            }

            return result;
        }

        private string GetInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Type: {_worker.GetType().FullName}; ");
            sb.AppendLine($"Name: {_worker.Name}; ");
            sb.AppendLine($"State: {_worker.State}");
            sb.AppendLine();

            return sb.ToString();
        }
    }
}
