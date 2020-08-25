using EasyNetQ;
using System;
using System.Reflection;
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

            var rpcHandle1 = bus.Respond<InvokeMethodRequest, InvokeMethodResponse>(
                this.ProcessMethodInvocation,
                configuration => configuration.WithQueueName(_worker.Name));

            _worker.WaitForStateChange(System.Threading.Timeout.Infinite, WorkerState.Disposed);

            rpcHandle1.Dispose();

            bus.Dispose();
        }

        private InvokeMethodResponse ProcessMethodInvocation(InvokeMethodRequest request)
        {
            try
            {
                var method = _worker.GetType().GetMethod(request.MethodName);
                if (method == null)
                {
                    throw new Exception($"Method '{request.MethodName}' not found.");
                }

                var parameters = BuildParameters(method, request.Arguments);
                var result = method.Invoke(_worker, parameters);
                var resultString = GetResultString(method, result);

                var response = new InvokeMethodResponse
                {
                    Result = resultString,
                };

                return response;
            }
            catch (TargetInvocationException ex)
            {
                var errorResponse = new InvokeMethodResponse
                {
                    Exception = ExceptionInfo.FromException(ex.InnerException),
                };

                return errorResponse;
            }
            catch (Exception ex)
            {
                var errorResponse = new InvokeMethodResponse
                {
                    Exception = ExceptionInfo.FromException(ex),
                };

                return errorResponse;
            }
        }

        private static string GetResultString(MethodInfo method, object result)
        {
            var returnType = method.ReturnType;
            if (returnType == typeof(void))
            {
                return null;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static object[] BuildParameters(MethodInfo method, string[] arguments)
        {
            var parameters = method.GetParameters();
            if (parameters.Length == 0)
            {
                return new object[] { };
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
