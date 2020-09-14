using System;
using System.Collections.Generic;
using TauCode.Labor;

namespace TauCode.Working.Jobs.Omicron
{
    public class OmicronJobManager : IJobManager
    {
        private readonly OmicronVice _vice;

        private OmicronJobManager()
        {
            _vice = new OmicronVice();
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
            throw new NotImplementedException();
        }

        public IReadOnlyList<string> GetNames() => throw new NotImplementedException();

        public IJob Get(string jobName)
        {
            throw new NotImplementedException();
        }
    }
}
