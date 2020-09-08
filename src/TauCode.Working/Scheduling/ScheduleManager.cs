using System;
using System.Collections.Generic;

// todo clean up
namespace TauCode.Working.Scheduling
{
    public class ScheduleManager : IScheduleManager
    {
        #region Fields

        private readonly object _lock;
        private bool _isStarted;
        private bool _isDisposed;
        private readonly ScheduleManagerHelper _helper;

        private readonly Dictionary<string, ScheduleRegistration> _registrations;
        private readonly HashSet<AutoStopWorkerBase> _workers;

        #endregion

        #region Constructor

        public ScheduleManager()
        {
            _lock = new object();
            _helper = new ScheduleManagerHelper();
            _registrations = new Dictionary<string, ScheduleRegistration>();
            _workers = new HashSet<AutoStopWorkerBase>();
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

        private string GenerateRegistrationId()
        {
            var result = Guid.NewGuid().ToString("N");
            return result;
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

        public string Register(AutoStopWorkerBase worker, ISchedule schedule)
        {
            if (worker == null)
            {
                throw new ArgumentNullException(nameof(worker));
            }

            if (schedule == null)
            {
                throw new ArgumentNullException(nameof(schedule));
            }

            lock (_lock)
            {
                this.CheckStarted();
                this.CheckNotDisposed();

                if (_workers.Contains(worker))
                {
                    throw new NotImplementedException(); // todo dup
                }

                var registrationId = this.GenerateRegistrationId();
                var registration = new ScheduleRegistration(registrationId, worker, schedule);

                _registrations.Add(registration.RegistrationId, registration);
                _workers.Add(worker);

                _helper.OnNewRegistration(registration); // todo: try/catch, remove on exception?

                return registrationId;
            }
        }

        public void ChangeSchedule(string registrationId, ISchedule schedule)
        {
            lock (_lock)
            {
                this.CheckStarted();
                this.CheckNotDisposed();

                throw new NotImplementedException();
            }
        }

        public void Remove(string registrationId)
        {
            lock (_lock)
            {
                this.CheckStarted();
                this.CheckNotDisposed();

                throw new NotImplementedException();
            }
        }

        public void Enable(string registrationId)
        {
            lock (_lock)
            {
                this.CheckStarted();
                this.CheckNotDisposed();

                throw new NotImplementedException();
            }
        }

        public void Disable(string registrationId)
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
