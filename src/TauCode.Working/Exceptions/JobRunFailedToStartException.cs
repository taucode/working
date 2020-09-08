using System;
using System.Runtime.Serialization;

namespace TauCode.Working.Exceptions
{
    [Serializable]
    public class JobRunFailedToStartException : JobException
    {
        public JobRunFailedToStartException(Exception inner)
            : base("Job run failed to start.", inner)
        {
        }

        protected JobRunFailedToStartException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
