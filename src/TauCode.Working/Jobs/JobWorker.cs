using System;
using System.Threading.Tasks;

namespace TauCode.Working.Jobs
{
    internal class JobWorker : WorkerBase
    {
        private readonly Func<Task> _taskCreator;

        internal JobWorker(Func<Task> taskCreator)
        {
            _taskCreator = taskCreator; // todo checks
        }

        protected override void StartImpl()
        {
            var task = _taskCreator().ContinueWith(this.Wat);
            this.ChangeState(WorkerState.Running);
        }

        private Task Wat(Task task)
        {
            this.Stop();
            return Task.CompletedTask;
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
