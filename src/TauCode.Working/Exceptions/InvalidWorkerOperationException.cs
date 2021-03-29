using System;
using System.Runtime.Serialization;

namespace TauCode.Working.Exceptions
{
    [Serializable]
    public class InvalidWorkerOperationException : InvalidOperationException
    {
        public InvalidWorkerOperationException()
        {
        }

        public InvalidWorkerOperationException(string message)
            : base(message)
        {
        }

        public InvalidWorkerOperationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public InvalidWorkerOperationException(string message, string workerName)
            : base(message)
        {
            this.WorkerName = workerName;
        }

        protected InvalidWorkerOperationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public string WorkerName { get; }
    }
}
