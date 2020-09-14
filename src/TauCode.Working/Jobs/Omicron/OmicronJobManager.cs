using System;
using System.Collections.Generic;
using TauCode.Labor;
using TauCode.Working.Exceptions;

namespace TauCode.Working.Jobs.Omicron
{
    public class OmicronJobManager : IJobManager
    {
        private readonly OmicronVice _vice;

        private OmicronJobManager()
        {
            _vice = new OmicronVice();
        }

        private void CheckJobName(string jobName, string jobNameParamName)
        {
            if (string.IsNullOrWhiteSpace(jobName))
            {
                throw new ArgumentException("Job name cannot be null or empty.", jobNameParamName);
            }
        }

        private void CheckCanWork()
        {
            if (this.IsDisposed)
            {
                throw new JobObjectDisposedException(typeof(IJobManager).FullName);
            }

            if (!this.IsRunning)
            {
                throw new InvalidJobOperationException($"'{typeof(IJobManager).FullName}' not started.");
            }
        }

        public static IJobManager CreateJobManager() => new OmicronJobManager();

        public void Dispose()
        {
            _vice.Dispose();
        }

        public void Start()
        {
            _vice.Start();
        }

        public bool IsRunning
        {
            get
            {
                return _vice.State == ProlState.Running;
            }
        }

        public bool IsDisposed
        {
            get
            {
                return _vice.IsDisposed;
            }
        }

        public IJob Create(string jobName)
        {
            this.CheckJobName(jobName, nameof(jobName));
            this.CheckCanWork();

            return _vice.CreateJob(jobName);
        }

        public IReadOnlyList<string> GetNames() => throw new NotImplementedException();

        public IJob Get(string jobName)
        {
            throw new NotImplementedException();
        }
    }
}
