using System;
using System.Runtime.Serialization;
using TauCode.Working.Exceptions;

namespace TauCode.Working.Labor.Exceptions
{
    [Serializable]
    public class InappropriateLaborerStateException : WorkingException
    {
        public InappropriateLaborerStateException(LaborerState laborerState)
            : this($"Inappropriate laborer state ({laborerState}).")
        {
        }

        public InappropriateLaborerStateException(string message)
            : base(message)
        {
        }

        public InappropriateLaborerStateException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected InappropriateLaborerStateException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
