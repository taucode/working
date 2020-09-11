using System;
using System.Collections.Generic;

namespace TauCode.Working.Jobs
{
    public interface IJobManager : IDisposable
    {
        void Start();

        IJob Create(string jobName);

        IReadOnlyList<string> GetJobNames();

        IJob Get(string jobName);

        void Remove(string jobName);
    }
}
