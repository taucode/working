﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// todo clean up
namespace TauCode.Working.Jobs
{
    public class JobManager : IJobManager
    {
        #region Nested

        // todo: internal, not public? here & in other private nested types.
        private class JobWorkerEntry
        {
            public JobWorkerEntry(string name, JobWorker worker, ISchedule schedule)
            {
                this.Name = name;
                this.Worker = worker;
                this.Schedule = schedule;
            }

            public string Name { get; }
            public JobWorker Worker { get; }
            public ISchedule Schedule { get; private set; }
            public bool IsEnabled { get; private set; }
        }

        #endregion

        #region Fields

        private readonly object _lock;
        private bool _isStarted;
        private bool _isDisposed;
        private readonly JobManagerHelper _helper;

        //private readonly Dictionary<string, ScheduleRegistration> _registrations;
        //private readonly HashSet<AutoStopWorkerBase> _workers;

        private readonly Dictionary<string, JobWorkerEntry> _entries;

        #endregion

        #region Constructor

        public JobManager()
        {
            _lock = new object();
            _helper = new JobManagerHelper(this);
            //_registrations = new Dictionary<string, ScheduleRegistration>();
            //_workers = new HashSet<AutoStopWorkerBase>();
            _entries = new Dictionary<string, JobWorkerEntry>();
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

        //private string GenerateRegistrationId()
        //{
        //    var result = Guid.NewGuid().ToString("N");
        //    return result;
        //}

        #endregion

        //#region IScheduleManager Members

        //public void Start()
        //{
        //    lock (_lock)
        //    {
        //        this.CheckNotStarted();
        //        this.CheckNotDisposed();

        //        _helper.Start();
        //        _isStarted = true;
        //    }
        //}

        //public string Register(AutoStopWorkerBase worker, ISchedule schedule)
        //{
        //    if (worker == null)
        //    {
        //        throw new ArgumentNullException(nameof(worker));
        //    }

        //    if (schedule == null)
        //    {
        //        throw new ArgumentNullException(nameof(schedule));
        //    }

        //    lock (_lock)
        //    {
        //        this.CheckStarted();
        //        this.CheckNotDisposed();

        //        if (_workers.Contains(worker))
        //        {
        //            throw new NotImplementedException(); // todo dup
        //        }

        //        var registrationId = this.GenerateRegistrationId();
        //        var registration = new ScheduleRegistration(registrationId, worker, schedule);

        //        _registrations.Add(registration.RegistrationId, registration);
        //        _workers.Add(worker);

        //        _helper.OnNewRegistration(registration); // todo: try/catch, remove on exception?

        //        return registrationId;
        //    }
        //}

        //public void ChangeSchedule(string registrationId, ISchedule schedule)
        //{
        //    lock (_lock)
        //    {
        //        this.CheckStarted();
        //        this.CheckNotDisposed();

        //        throw new NotImplementedException();
        //    }
        //}

        //public void Remove(string registrationId)
        //{
        //    lock (_lock)
        //    {
        //        this.CheckStarted();
        //        this.CheckNotDisposed();

        //        throw new NotImplementedException();
        //    }
        //}

        //public void Enable(string registrationId)
        //{
        //    lock (_lock)
        //    {
        //        this.CheckStarted();
        //        this.CheckNotDisposed();

        //        throw new NotImplementedException();
        //    }
        //}

        //public void Disable(string registrationId)
        //{
        //    lock (_lock)
        //    {
        //        this.CheckStarted();
        //        this.CheckNotDisposed();

        //        throw new NotImplementedException();
        //    }
        //}

        //#endregion

        #region IDisposable Members

        public void Dispose()
        {
            lock (_lock)
            {
                this.CheckNotDisposed();

                _helper.Dispose();
                _isDisposed = true;

                // todo: dispose workers.
            }
        }

        #endregion

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

        public void RegisterJob(string jobName, Func<Task> jobTaskCreator, ISchedule jobSchedule)
        {
            if (jobName == null)
            {
                throw new ArgumentNullException(nameof(jobName));
            }

            if (jobTaskCreator == null)
            {
                throw new ArgumentNullException(nameof(jobTaskCreator));
            }

            if (jobSchedule == null)
            {
                throw new ArgumentNullException(nameof(jobSchedule));
            }

            lock (_lock)
            {
                this.CheckStarted();
                this.CheckNotDisposed();

                var entry = new JobWorkerEntry(jobName, new JobWorker(jobTaskCreator), jobSchedule);
                _entries.Add(entry.Name, entry);
            }

            _helper.Reschedule(jobName, jobSchedule); // todo: try/catch, remove on exception?
        }

        public void ChangeJobSchedule(string jobName, ISchedule newJobSchedule)
        {
            throw new NotImplementedException();
        }

        public void EnableJob(string jobName, bool enable)
        {
            throw new NotImplementedException();
        }

        public void RemoveJob(string jobName)
        {
            throw new NotImplementedException();
        }

        #region Internal

        internal void StartJob(string jobName)
        {
            IWorker worker;

            lock (_lock)
            {
                var entry = _entries[jobName];
                worker = entry.Worker;
            }

            worker.Start();
        }

        #endregion

        internal ISchedule GetSchedule(string jobName)
        {
            // todo checks

            lock (_lock)
            {
                return _entries[jobName].Schedule;
            }
        }
    }
}