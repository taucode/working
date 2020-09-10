using System;
using TauCode.Infrastructure.Time;

namespace TauCode.Working.Jobs
{
    internal class DueTimeInfoBuilder
    {
        private readonly object _lock;

        private DueTimeType _type;
        private DateTime _dueTime;

        public DueTimeInfoBuilder()
        {
            _lock = new object();
        }

        internal void UpdateBySchedule(ISchedule schedule)
        {
            lock (_lock)
            {
                _type = DueTimeType.BySchedule;
                var now = TimeProvider.GetCurrent();
                var dueTime = schedule.GetDueTimeAfter(now);
                _dueTime = dueTime;
            }
        }

        internal void UpdateManually(DateTime manualDueTime)
        {
            throw new NotImplementedException();
        }

        public DueTimeInfo Build()
        {
            lock (_lock)
            {
                return new DueTimeInfo(_type, _dueTime);
            }
        }
    }
}
