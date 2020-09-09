using System;

namespace TauCode.Working.Jobs
{
    public readonly struct DueTimeInfo
    {
        public DueTimeInfo(DueTimeType type, DateTime dueTime)
        {
            this.Type = type;
            this.DueTime = dueTime;
        }

        public DueTimeType Type { get; }
        public DateTime DueTime { get; }
    }
}
