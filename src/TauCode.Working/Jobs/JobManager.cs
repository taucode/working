using System;
using System.Collections.Generic;

// todo clean up
namespace TauCode.Working.Jobs
{
    public class JobManager : IJobManager
    {
        #region Fields

        private readonly Vice _vice;

        #endregion

        #region Constructor

        public JobManager()
        {
            _vice = new Vice(this)
            {
                Name = typeof(Vice).FullName,
            };
        }

        #endregion

        #region IJobManager Members

        public void Start()
        {
            _vice.Start(); // todo: gracefully handle exception when _vice throws them
        }

        public IJob Create(string jobName) => _vice.CreateJob(jobName);

        public IReadOnlyList<string> GetJobNames() => _vice.GetJobNames();

        public IJob Get(string jobName) => _vice.GetJob(jobName);

        public void Remove(string jobName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException(); // todo ut. dispose vice and employees.

            //lock (_lock)
            //{
            //    throw new NotImplementedException();
            //    //this.CheckNotDisposed();

            //    //_helper.Dispose();
            //    //_isDisposed = true;

            //    //foreach (var entry in _entries)
            //    //{
            //    //    entry.Value.Worker.Dispose();
            //    //}

            //    //// todo: dispose workers.
            //}
        }

        #endregion
    }
}
