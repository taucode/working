using System;
using TauCode.Infrastructure.Time;
using TauCode.Working.Schedules;

// todo clean
namespace TauCode.Working.Jobs.Instruments
{
    internal class DueTimeHolder
    {
        private readonly object _lock;
        private DateTimeOffset _scheduleDueTime;
        private DateTimeOffset? _overriddenDueTime;
        private ISchedule _schedule;

        internal DueTimeHolder(ISchedule schedule)
        {
            _schedule = schedule;
            _lock = new object();
            this.UpdateScheduleDueTime();
        }

        internal void UpdateScheduleDueTime()
        {
            var now = TimeProvider.GetCurrent();
            lock (_lock)
            {
                _scheduleDueTime = _schedule.GetDueTimeAfter(now.AddTicks(1));
            }
        }

        internal ISchedule Schedule
        {
            get
            {
                lock (_lock)
                {
                    return _schedule;
                }
            }
            set
            {
                lock (_lock)
                {
                    _schedule = value;
                    _overriddenDueTime = null;
                    this.UpdateScheduleDueTime();
                }
            }
        }

        internal DateTimeOffset? OverriddenDueTime
        {
            get
            {
                lock (_lock)
                {
                    return _overriddenDueTime;
                }
            }
            set
            {
                lock (_lock)
                {
                    _overriddenDueTime = value;
                }
            }
        }

        internal DueTimeInfo GetDueTimeInfo() => new DueTimeInfo(_scheduleDueTime, _overriddenDueTime);
    }
}
