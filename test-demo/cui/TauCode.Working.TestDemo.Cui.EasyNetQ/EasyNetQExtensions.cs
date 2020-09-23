using EasyNetQ;
using System;

namespace TauCode.Working.TestDemo.Cui.EasyNetQ
{
    public static class EasyNetQExtensions
    {
        private static string BuildWorkerQueueName<TRequest, TResponse>(string workerName)
        {
            var queueName = $"{typeof(TRequest).FullName}, {typeof(TResponse).FullName}: {workerName}";
            return queueName;
        }

        public static IDisposable RespondForWorker<TRequest, TResponse>(
            this IBus bus,
            Func<TRequest, TResponse> responder,
            string workerName)
            where TRequest : class
            where TResponse : class
        {
            if (string.IsNullOrWhiteSpace(workerName))
            {
                throw new ArgumentException($"'{nameof(workerName)}' cannot be empty.", nameof(workerName));
            }

            var queueName = BuildWorkerQueueName<TRequest, TResponse>(workerName);
            
            var result = bus.Respond(responder, configuration => configuration.WithQueueName(queueName));
            return result;
        }

        public static TResponse RequestForWorker<TRequest, TResponse>(
            this IBus bus,
            TRequest request,
            string workerName)
            where TRequest : class
            where TResponse : class
        {
            if (string.IsNullOrWhiteSpace(workerName))
            {
                throw new ArgumentException($"'{nameof(workerName)}' cannot be empty.", nameof(workerName));
            }

            var queueName = BuildWorkerQueueName<TRequest, TResponse>(workerName);

            var response =  bus.Request<TRequest, TResponse>(
                request,
                configuration => configuration.WithQueueName(queueName));

            return response;
        }
    }
}
