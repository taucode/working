using System;

namespace TauCode.Working.ZetaOld.Workers
{
    public interface IZetaTimeoutWorker
    {
        /// <summary>
        /// Timeout between work actions
        /// </summary>
        public TimeSpan Timeout { get; set; }
    }
}
