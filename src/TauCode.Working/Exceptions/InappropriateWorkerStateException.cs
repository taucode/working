using System;
using System.Runtime.Serialization;

namespace TauCode.Working.Exceptions
{
    [Serializable]
    public class InappropriateWorkerStateException : WorkingException
    {
        public InappropriateWorkerStateException(WorkerState workerState)
            : this($"Inappropriate worker state ({workerState}).")
        {
        }

        public InappropriateWorkerStateException(string message)
            : base(message)
        {
        }

        public InappropriateWorkerStateException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected InappropriateWorkerStateException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
