using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TauCode.Working.Exceptions
{
    [Serializable]
    public class ForbiddenWorkerStateException : WorkingException
    {
        public ForbiddenWorkerStateException(
            string workerName,
            IEnumerable<WorkerState> acceptableStates,
            WorkerState actualState,
            string message)
            : base(message)
        {
            if (acceptableStates == null)
            {
                throw new ArgumentNullException(nameof(acceptableStates));
            }

            this.WorkerName = workerName;
            this.ActualState = actualState;
            this.AcceptableStates = acceptableStates.ToList();
        }
        protected ForbiddenWorkerStateException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public string WorkerName { get; }

        public IReadOnlyList<WorkerState> AcceptableStates { get; }

        public WorkerState ActualState { get; }
    }
}
