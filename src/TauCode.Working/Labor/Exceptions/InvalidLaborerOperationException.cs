using System;
using System.Runtime.Serialization;

namespace TauCode.Working.Labor.Exceptions
{
    [Serializable]
    public class InvalidLaborerOperationException : InvalidOperationException
    {
        public InvalidLaborerOperationException()
        {
        }

        public InvalidLaborerOperationException(string message)
            : base(message)
        {
        }

        public InvalidLaborerOperationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected InvalidLaborerOperationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
