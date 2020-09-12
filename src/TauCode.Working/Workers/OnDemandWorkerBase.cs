using System;

namespace TauCode.Working.Workers
{
    public class OnDemandWorkerBase : WorkerBase
    {
        /// <summary>
        /// Call this method as one of the first methods (right after argument checks) in the descendant class's job-doing methods.
        /// </summary>
        protected void CheckCanDoJob()
        {
            var message = "Check before doing the job.";
            this.LogDebug(message);
            this.CheckState(message, WorkerState.Running);
        }

        protected override void StartImpl()
        {
            this.ChangeState(WorkerState.Running);
        }

        protected override void PauseImpl()
        {
            throw new NotSupportedException("Pausing is not supported.");
        }

        protected override void ResumeImpl()
        {
            throw new NotSupportedException("Resuming is not supported.");
        }

        protected override void StopImpl()
        {
            this.ChangeState(WorkerState.Stopped);
        }

        protected override void DisposeImpl()
        {
            this.ChangeState(WorkerState.Disposed);
        }
    }
}
