using System;

namespace TauCode.Working.Jobs
{
    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(decimal percentCompleted)
        {
            this.PercentCompleted = percentCompleted;
        }

        public decimal PercentCompleted { get; }
    }
}
