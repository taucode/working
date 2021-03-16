using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TauCode.Working
{
    public abstract class QueueWorkerBase<TAssignment> : LoopWorkerBase
    {
        private readonly Queue<TAssignment> _assignments;
        private readonly object _lock;

        protected QueueWorkerBase()
        {
            _assignments = new Queue<TAssignment>();
            _lock = new object();
        }

        public void AddAssignment(TAssignment assignment)
        {
            this.CheckAssignment(assignment);

            lock (_lock)
            {
                _assignments.Enqueue(assignment);
            }

            this.AdvanceWorkGeneration();
        }

        protected abstract Task DoAssignment(TAssignment assignment, CancellationToken cancellationToken);

        protected virtual void CheckAssignment(TAssignment assignment)
        {
            // idle.
        }

        protected override async Task<TimeSpan> DoWork(CancellationToken cancellationToken)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                TAssignment assignment;

                lock (_lock)
                {
                    if (_assignments.Count > 0)
                    {
                        assignment = _assignments.Dequeue();
                    }
                    else
                    {
                        return VeryLongVacation; // no assignments to work on.
                    }
                }

                try
                {
                    await this.DoAssignment(assignment, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw; // task was canceled for reason
                }
                catch (Exception ex)
                {
                    // todo
                    //Log.Error(ex, $"Method '{nameof(DoAssignment)}' threw an exception.");
                    // let's continue working on assignments (if still any).
                }
            }
        }
    }
}
