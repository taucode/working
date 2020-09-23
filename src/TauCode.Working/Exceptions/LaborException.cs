using System;
using System.Runtime.Serialization;

namespace TauCode.Working.Exceptions
{
    [Serializable]
    public class LaborException : Exception
    {

        public LaborException()
        {
        }

        public LaborException(string message) : base(message)
        {
        }

        public LaborException(string message, Exception inner) : base(message, inner)
        {
        }

        protected LaborException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
