using System;

namespace TauCode.Working.Workers
{
    public interface ITimeoutWorker
    {
        /// <summary>
        /// Timeout between work actions
        /// </summary>
        public TimeSpan Timeout { get; set; }
    }
}
