using System;
using System.Runtime.Serialization;

namespace TauCode.Working.Exceptions
{
    [Serializable]
    public class InappropriateWorkerStateException : LaborException
    {
        public InappropriateWorkerStateException()
        {
        }

        public InappropriateWorkerStateException(string message) : base(message)
        {
        }

        public InappropriateWorkerStateException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InappropriateWorkerStateException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
