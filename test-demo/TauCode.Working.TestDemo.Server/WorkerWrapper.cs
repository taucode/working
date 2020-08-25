using EasyNetQ;
using System;
using TauCode.Working.TestDemo.Common;

namespace TauCode.Working.TestDemo.Server
{
    public class WorkerWrapper
    {
        private readonly IWorker _worker;
        private readonly string _connectionString;

        public WorkerWrapper(IWorker worker, string connectionString)
        {
            _worker = worker;
            _connectionString = connectionString;
        }

        public void Run()
        {
            var bus = RabbitHutch.CreateBus(_connectionString);
            bus.Respond<InvokeMethodRequest, InvokeMethodResponse>(this.ProcessMethodInvocation);
        }

        private InvokeMethodResponse ProcessMethodInvocation(InvokeMethodRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
