using System;

namespace TauCode.Working
{
    public class AutoStopWorkerBase : WorkerBase
    {
        #region Overridden

        protected override void StartImpl()
        {
            throw new NotImplementedException();
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

        #endregion
    }
}
