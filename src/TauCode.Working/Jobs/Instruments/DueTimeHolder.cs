using System;
using TauCode.Infrastructure.Time;
using TauCode.Working.Exceptions;
using TauCode.Working.Schedules;

namespace TauCode.Working.Jobs.Instruments
{
    internal class DueTimeHolder : IDisposable
    {
        #region Fields

        private ISchedule _schedule;
        private DateTimeOffset? _overriddenDueTime;

        private DateTimeOffset _scheduleDueTime; // calculated

        private bool _isDisposed;
        private readonly string _jobName;

        private readonly object _lock;

        private readonly ObjectLogger _logger;

        #endregion

        #region Constructor

        internal DueTimeHolder(string jobName)
        {
            _jobName = jobName;
            _schedule = NeverSchedule.Instance;
            _lock = new object();
            this.UpdateScheduleDueTime();

            _logger = new ObjectLogger(this, _jobName);
        }

        #endregion

        #region Private

        private void CheckNotDisposed()
        {
            lock (_lock)
            {
                if (_isDisposed)
                {
                    throw new JobObjectDisposedException(_jobName);
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
                if (_isDisposed)
                {
                    // todo: write to log about strange attempt.
                    return;
                }

                try
                {
                    _scheduleDueTime = _schedule.GetDueTimeAfter(now.AddTicks(1));
                }
                catch (Exception ex)
                {
                    _scheduleDueTime = JobExtensions.Never;

                    _logger.Warning(
                        "An exception was thrown on attempt to calculate due time",
                        nameof(UpdateScheduleDueTime),
                        ex);
                }
            }
        }

        internal DueTimeInfo GetDueTimeInfo()
        {
            lock (_lock)
            {
                return new DueTimeInfo(_scheduleDueTime, _overriddenDueTime);
            }
        }

        internal void EnableLogging(bool enable)
        {
            _logger.IsEnabled = enable;
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
