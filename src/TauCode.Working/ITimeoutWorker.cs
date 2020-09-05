using System;

namespace TauCode.Working
{
    public interface ITimeoutWorker
    {
        /// <summary>
        /// Timeout between work actions
        /// </summary>
        public TimeSpan Timeout { get; set; }
    }
}
