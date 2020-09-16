using System;

namespace TauCode.Working.Jobs
{
    internal readonly struct DueTimeInfoForVice
    {
        internal DueTimeInfoForVice(DateTimeOffset dueTime, bool isOverridden)
        {
            this.DueTime = dueTime;
            this.IsOverridden = isOverridden;
        }

        internal DateTimeOffset DueTime { get; }
        internal bool IsOverridden { get; }
    }
}
