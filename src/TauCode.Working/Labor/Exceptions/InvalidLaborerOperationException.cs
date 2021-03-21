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

        public InvalidLaborerOperationException(string message, string laborerName)
            : base(message)
        {
            this.LaborerName = laborerName;
        }

        protected InvalidLaborerOperationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public string LaborerName { get; }
    }
}
