using System;
using System.Runtime.Serialization;

namespace TauCode.Working.Exceptions
{
    [Serializable]
    public class JobException : Exception
    {
        public JobException()
        {
        }

        public JobException(string message)
            : base(message)
        {
        }

        public JobException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected JobException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
