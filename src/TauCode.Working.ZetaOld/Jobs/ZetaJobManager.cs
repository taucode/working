using System;
using System.Collections.Generic;
using TauCode.Working.Exceptions;
using TauCode.Working.Jobs;
using TauCode.Working.ZetaOld.Workers;

namespace TauCode.Working.ZetaOld.Jobs
{
    public class ZetaJobManager : IJobManager
    {
        #region Constants

        private static readonly List<ZetaWorkerState> StatesStopped = new List<ZetaWorkerState>
        {
            ZetaWorkerState.Stopped,
        };

        #endregion

        #region Fields

        private readonly ZetaVice _vice;

        #endregion

        #region Constructor

        private ZetaJobManager()
        {
            _vice = new ZetaVice
            {
                Name = typeof(ZetaVice).FullName,
            };
        }

        public static IJobManager CreateJobManager() => new ZetaJobManager();

        #endregion

        #region Private

        private void DebugPulse()
        {
            _vice.DebugPulse();
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

        #endregion

        #region IJobManager Members

        public void Start()
        {
            throw new NotImplementedException();
            //try
            //{
            //    _vice.Start();
            //}
            //catch (ForbiddenWorkerStateException ex) when (ex.WorkerName == typeof(Vice).FullName)
            //{
            //    if (
            //        ex.ActualState == WorkerState.Disposed)
            //    {
            //        throw new JobObjectDisposedException(typeof(IJobManager).FullName);
            //    }
            //    else if (
            //        ex.ActualState == WorkerState.Running &&
            //        TauCode.Extensions.Lab.CollectionExtensionsLab.ListsAreEquivalent(ex.AcceptableStates, StatesStopped, false))
            //    {
            //        throw new InvalidJobOperationException($"'{typeof(IJobManager).FullName}' is already running");
            //    }

            //    throw;
            //}
        }

        public bool IsRunning => _vice.WorkerIsRunning();

        public bool IsDisposed => _vice.WorkerIsDisposed();

        public IJob Create(string jobName)
        {
            this.CheckJobName(jobName, nameof(jobName));
            this.CheckCanWork();

            return _vice.CreateJob(jobName);
        }

        public IReadOnlyList<string> GetNames()
        {
            this.CheckCanWork();
            return _vice.GetJobNames();
        }

        public IJob Get(string jobName)
        {
            this.CheckJobName(jobName, nameof(jobName));
            this.CheckCanWork();

            throw new NotImplementedException();
            //return _vice.GetJob(jobName);
        }

        #endregion

        #region IDisposable Members

        public void Dispose() => _vice.Dispose();

        #endregion
    }
}
