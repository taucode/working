using System;
using TauCode.Infrastructure.Time;
using TauCode.Working.Schedules;

namespace TauCode.Working.Jobs.Instruments
{
    internal class ScheduleHolder
    {
        private readonly Vice _vice;
        private readonly object _lock;
        private DateTimeOffset _scheduleDueTime;
        private DateTimeOffset? _overriddenDueTime;
        private ISchedule _schedule;

        internal ScheduleHolder(
            Vice vice,
            ISchedule schedule)
        {
            _vice = vice;
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
                Console.WriteLine($">>> {_scheduleDueTime.Second:D2}:{_scheduleDueTime.Millisecond:D3}");
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
                    this.UpdateScheduleDueTime();
                }
            }
        }

        internal DueTimeInfo GetDueTimeInfo() => new DueTimeInfo(_scheduleDueTime, _overriddenDueTime);
    }
}
