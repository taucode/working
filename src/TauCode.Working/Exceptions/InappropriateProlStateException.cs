using System;
using System.Runtime.Serialization;

namespace TauCode.Working.Exceptions
{
    [Serializable]
    public class InappropriateProlStateException : LaborException
    {
        public InappropriateProlStateException()
        {
        }

        public InappropriateProlStateException(string message) : base(message)
        {
        }

        public InappropriateProlStateException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InappropriateProlStateException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
