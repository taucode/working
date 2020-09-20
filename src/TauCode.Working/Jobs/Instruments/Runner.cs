using System;
using System.Threading;
using TauCode.Working.Exceptions;

namespace TauCode.Working.Jobs.Instruments
{
    internal class Runner : IDisposable
    {
        #region Fields

        private bool _isEnabled;
        private bool _isDisposed;

        private RunContext _runContext;

        private readonly object _lock;

        private readonly ObjectLogger _logger;

        #endregion

        #region Constructor

        internal Runner(string jobName)
        {
            this.JobName = jobName;

            this.JobPropertiesHolder = new JobPropertiesHolder(this.JobName);
            this.DueTimeHolder = new DueTimeHolder(this.JobName);
            this.JobRunsHolder = new JobRunsHolder();

            _lock = new object();

            _logger = new ObjectLogger(this, jobName);
        }

        #endregion

        #region Private

        private void CheckNotDisposed()
        {
            lock (_lock)
            {
                if (_isDisposed)
                {
                    throw new JobObjectDisposedException(this.JobName);
                }
            }
        }

        private RunContext Run(JobStartReason startReason, CancellationToken? token)
        {
            // always guarded by '_lock'
            var runContext = new RunContext(this, startReason, token);
            var startedRunContext = runContext.Start();
            return startedRunContext;
        }

        #endregion

        #region Internal

        internal string JobName { get; }

        internal bool IsEnabled
        {
            get
            {
                lock (_lock)
                {
                    return _isEnabled;
                }
            }
            set
            {
                lock (_lock)
                {
                    this.CheckNotDisposed();
                    _isEnabled = value;
                }
            }
        }

        internal bool IsRunning
        {
            get
            {
                lock (_lock)
                {
                    return _runContext != null;
                }
            }
        }

        internal JobPropertiesHolder JobPropertiesHolder { get; }

        internal DueTimeHolder DueTimeHolder { get; }

        internal JobRunsHolder JobRunsHolder { get; }

        internal bool IsDisposed
        {
            get
            {
                lock (_lock)
                {
                    return _isDisposed;
                }
            }
        }

        internal DueTimeInfo? GetDueTimeInfoForVice(bool future)
        {
            lock (_lock)
            {
                if (_isDisposed)
                {
                    return null;
                }

                if (future)
                {
                    this.DueTimeHolder.UpdateScheduleDueTime();
                }

                return this.DueTimeHolder.GetDueTimeInfo();
            }
        }

        internal bool Cancel()
        {
            lock (_lock)
            {
                if (_runContext == null)
                {
                    return false;
                }

                _runContext.Cancel();
                return true;
            }
        }

        internal JobStartResult Start(JobStartReason startReason, CancellationToken? token)
        {
            if (startReason == JobStartReason.Force)
            {
                lock (_lock)
                {
                    if (this.IsRunning)
                    {
                        throw new NotImplementedException();
                    }

                    if (!this.IsEnabled)
                    {
                        throw new JobException($"Job '{this.JobName}' is disabled.");
                    }

                    _runContext = this.Run(startReason, token);

                    if (_runContext == null)
                    {
                        return JobStartResult.CompletedSynchronously;
                    }

                    return JobStartResult.Started;
                }
            }
            else
            {
                lock (_lock)
                {
                    if (this.IsRunning)
                    {
                        return JobStartResult.AlreadyRunning;
                    }

                    if (!this.IsEnabled)
                    {
                        return JobStartResult.Disabled;
                    }

                    _runContext = this.Run(startReason, token);

                    // started via due time, so clear overridden due time.
                    this.DueTimeHolder.OverriddenDueTime = null;

                    if (_runContext == null)
                    {
                        return JobStartResult.CompletedSynchronously;
                    }

                    return JobStartResult.Started;
                }
            }
        }

        internal JobInfo GetInfo(int? maxRunCount)
        {
            var tuple = this.JobRunsHolder.Get(maxRunCount);
            var currentRun = tuple.Item1;
            var runs = tuple.Item2;

            var dueTimeInfo = this.DueTimeHolder.GetDueTimeInfo();

            return new JobInfo(
                currentRun,
                dueTimeInfo.GetEffectiveDueTime(),
                dueTimeInfo.IsDueTimeOverridden(),
                this.JobRunsHolder.Count,
                runs);
        }

        internal void OnTaskEnded()
        {
            lock (_lock)
            {
                _runContext = null;
            }
        }

        internal bool IsLoggingEnabled => _logger.IsEnabled;

        internal void EnableLogging(bool enable)
        {
            _logger.IsEnabled = enable;

            this.JobPropertiesHolder.EnableLogging(enable);
            this.DueTimeHolder.EnableLogging(enable);
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

                try
                {
                    _runContext?.Cancel();
                    _runContext = null;
                }
                catch
                {
                    // dismiss; Dispose shouldn't throw
                }

                this.JobPropertiesHolder.Dispose();
                this.DueTimeHolder.Dispose();
            }
        }

        #endregion
    }
}
