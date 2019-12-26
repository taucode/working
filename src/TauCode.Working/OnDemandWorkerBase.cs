using System;

namespace TauCode.Working.Lab
{
    public abstract class OnDemandWorkerBase : WorkerBase
    {
        //protected void CheckCanDoJob()
        //{
        //    this.CheckStateForOperation(WorkerState.Running);
        //}

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
