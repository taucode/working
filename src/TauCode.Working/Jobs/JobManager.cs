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
            _vice = new Vice()
            {
                Name = typeof(Vice).FullName,
            };
        }

        #endregion

        #region Private

        private void DebugPulse()
        {
            _vice.DebugPulse();
        }

        #endregion

        #region IJobManager Members

        public void Start() => _vice.Start(); // todo: gracefully handle exception when _vice throws them

        public bool IsRunning => throw new NotImplementedException();

        public bool IsDisposed => throw new NotImplementedException();

        public IJob Create(string jobName) => _vice.CreateJob(jobName);

        public IReadOnlyList<string> GetNames() => _vice.GetJobNames();

        public IJob Get(string jobName) => _vice.GetJob(jobName);

        public void Remove(string jobName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable Members

        public void Dispose() => _vice.Dispose();

        #endregion
    }
}
