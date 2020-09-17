﻿using System;
using TauCode.Infrastructure.Time;
using TauCode.Working.Schedules;

// todo clean
namespace TauCode.Working.Jobs.Instruments
{
    internal class DueTimeHolder : IDisposable
    {
        #region Fields

        private ISchedule _schedule;
        private DateTimeOffset? _overriddenDueTime;

        private DateTimeOffset _scheduleDueTime; // calculated

        private bool _isDisposed;

        private readonly object _lock;

        #endregion

        #region Constructor

        internal DueTimeHolder()
        {
            _schedule = NeverSchedule.Instance;
            _lock = new object();
            this.UpdateScheduleDueTime();
        }

        #endregion

        #region Private

        private void CheckNotDisposed()
        {
            lock (_lock)
            {
                if (_isDisposed)
                {
                    throw new NotImplementedException(); // todo
                }
            }
        }

        #endregion

        #region Internal

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
                    this.CheckNotDisposed();
                    _schedule = value ?? throw new ArgumentNullException(nameof(IJob.Schedule));
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
                    this.CheckNotDisposed();
                    _overriddenDueTime = value;
                }
            }
        }

        internal void UpdateScheduleDueTime()
        {
            var now = TimeProvider.GetCurrent();
            lock (_lock)
            {
                // todo: check not disposed?
                _scheduleDueTime = _schedule.GetDueTimeAfter(now.AddTicks(1));
            }
        }

        internal DueTimeInfo GetDueTimeInfo()
        {
            lock (_lock)
            {
                return new DueTimeInfo(_scheduleDueTime, _overriddenDueTime);
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            lock (_lock)
            {
                if (_isDisposed)
                {
                    return;
                }

                _isDisposed = true;
            }
        }

        #endregion
    }
}
