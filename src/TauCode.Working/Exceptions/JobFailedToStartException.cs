using System;
using System.Runtime.Serialization;

namespace TauCode.Working.Exceptions
{
    [Serializable]
    public class JobFailedToStartException : JobException
    {
        public JobFailedToStartException(Exception inner)
            : base("Job run failed to start.", inner)
        {
        }

        protected JobFailedToStartException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
