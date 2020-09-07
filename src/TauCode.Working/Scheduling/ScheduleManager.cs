using System;
using System.Collections.Generic;

namespace TauCode.Working.Scheduling
{
    public class ScheduleManager : IScheduleManager
    {
        #region Fields

        private readonly object _lock;
        private bool _isStarted;
        private bool _isDisposed;
        private readonly ScheduleManagerHelper _helper;

        #endregion

        #region Constructor

        public ScheduleManager()
        {
            _lock = new object();
            _helper = new ScheduleManagerHelper();
        }

        #endregion

        #region Private

        private void CheckNotStarted()
        {
            if (_isStarted)
            {
                throw new InvalidOperationException("Manager already started.");
            }
        }

        private void CheckStarted()
        {
            if (!_isStarted)
            {
                throw new InvalidOperationException("Manager not started.");
            }
        }

        private void CheckNotDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("Manager was disposed.");
            }
        }

        #endregion

        #region IScheduleManager Members

        public void Start()
        {
            lock (_lock)
            {
                this.CheckNotStarted();
                this.CheckNotDisposed();

                _helper.Start();
                _isStarted = true;
            }
        }

        public string RegisterWorker(IScheduledWorker scheduledWorker)
        {
            lock (_lock)
            {
                this.CheckStarted();
                this.CheckNotDisposed();

                throw new NotImplementedException();
            }
        }

        public void UnregisterWorker(IScheduledWorker scheduledWorker)
        {
            lock (_lock)
            {
                this.CheckStarted();
                this.CheckNotDisposed();

                throw new NotImplementedException();
            }
        }

        public void UnregisterWorker(string registrationId)
        {
            lock (_lock)
            {
                this.CheckStarted();
                this.CheckNotDisposed();

                throw new NotImplementedException();
            }
        }

        public IReadOnlyDictionary<string, ScheduledWorkerRegistration> GetWorkers()
        {
            lock (_lock)
            {
                this.CheckStarted();
                this.CheckNotDisposed();

                throw new NotImplementedException();
            }
        }

        public void EnableSchedule(string registrationId)
        {
            lock (_lock)
            {
                this.CheckStarted();
                this.CheckNotDisposed();

                throw new NotImplementedException();
            }
        }

        public void DisableSchedule(string registrationId)
        {
            lock (_lock)
            {
                this.CheckStarted();
                this.CheckNotDisposed();

                throw new NotImplementedException();
            }
        }


        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            lock (_lock)
            {
                this.CheckNotDisposed();

                _helper.Dispose();
                _isDisposed = true;
            }
        }

        #endregion
    }
}
