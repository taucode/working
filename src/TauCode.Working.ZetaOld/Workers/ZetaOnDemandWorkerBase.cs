using System;

namespace TauCode.Working.ZetaOld.Workers
{
    public class ZetaOnDemandWorkerBase : ZetaWorkerBase
    {
        /// <summary>
        /// Call this method as one of the first methods (right after argument checks) in the descendant class's job-doing methods.
        /// </summary>
        protected void CheckCanDoJob()
        {
            var message = "Check before doing the job.";
            //this.LogDebug(message);
            this.GetLogger().Debug(message, nameof(CheckCanDoJob));

            this.CheckState(message, ZetaWorkerState.Running);
        }

        protected override void StartImpl()
        {
            this.ChangeState(ZetaWorkerState.Running);
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
            this.ChangeState(ZetaWorkerState.Stopped);
        }

        protected override void DisposeImpl()
        {
            this.ChangeState(ZetaWorkerState.Disposed);
        }
    }
}
