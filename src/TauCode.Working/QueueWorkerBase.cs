using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TauCode.Working
{
    public abstract class QueueWorkerBase<TAssignment> : LoopWorkerBase, IQueueWorker<TAssignment>
    {
        #region Constants

        private const int AssignmentsQueuedSignalIndex = 1;

        private const int VacationTimeoutMilliseconds = 10;

        #endregion

        #region Fields

        private readonly Queue<TAssignment> _assignments;
        private readonly object _dataLock;
        private AutoResetEvent _dataSignal; // disposed by LoopWorkerBase.Shutdown

        #endregion

        #region Constructor

        protected QueueWorkerBase()
        {
            _assignments = new Queue<TAssignment>();
            _dataLock = new object();
        }

        #endregion

        #region Private

        private bool TryGetAssignment(out TAssignment assignment)
        {
            lock (_dataLock)
            {
                if (_assignments.Count == 0)
                {
                    assignment = default;
                    return false;
                }
                else
                {
                    assignment = _assignments.Dequeue();
                    return true;
                }
            }
        }

        #endregion

        #region Abstract

        protected abstract Task DoAssignmentAsync(TAssignment assignment);

        #endregion

        #region Overridden

        protected override async Task<WorkFinishReason> DoWorkAsyncImpl()
        {
            while (true)
            {
                var gotAssignment = this.TryGetAssignment(out var assignment);
                if (!gotAssignment)
                {
                    // queue is empty - work is done.
                    return WorkFinishReason.WorkIsDone;
                }

                // let's work on assignment.
                try
                {
                    // loop must not break due to an exception
                    await this.DoAssignmentAsync(assignment);
                }
                catch (Exception ex)
                {
                    this.LogError($"Assignment {assignment} caused an exception: {ex}.");
                }

                // got more assignments?
                if (this.Backlog > 0)
                {
                    // more work is coming
                }
                else
                {
                    // backlog is empty, let's have a vacation then
                    return WorkFinishReason.WorkIsDone;
                }

                // ok, we have more work to do, but let's check if we didn't receive some signals

                var signalIndex = this.WaitForControlSignalWithExtraSignals(0);

                switch (signalIndex)
                {
                    case ControlSignalIndex:
                        // got control signal, let's stop working
                        return WorkFinishReason.GotControlSignal;

                    case AssignmentsQueuedSignalIndex:
                        // got more work, nice; continue loop
                        break;

                    case WaitHandle.WaitTimeout:
                        // no signals received, continue loop
                        break;

                    default:
                        throw this.CreateInternalErrorException(); // should never happen.
                }
            }
        }

        protected override Task<VacationFinishReason> TakeVacationAsyncImpl()
        {
            while (true)
            {
                var signalIndex = this.WaitForControlSignalWithExtraSignals(VacationTimeoutMilliseconds);

                switch (signalIndex)
                {
                    case ControlSignalIndex:
                        // got control signal, let's stop vacation
                        return Task.FromResult(VacationFinishReason.GotControlSignal);

                    case AssignmentsQueuedSignalIndex:
                        // new work arrived, stop vacation
                        return Task.FromResult(VacationFinishReason.NewWorkArrived);

                    case WaitHandle.WaitTimeout:
                        // no signals received, let's continue enjoying the vacation
                        break;

                    default:
                        throw this.CreateInternalErrorException(); // should never happen.
                }
            }
        }

        protected override IList<AutoResetEvent> CreateExtraSignals()
        {
            _dataSignal = new AutoResetEvent(false);
            return new[] { _dataSignal };
        }

        protected override void Shutdown(WorkerState shutdownState)
        {
            base.Shutdown(shutdownState); // _dataSignal is disposed here.
            _dataSignal = null;
        }
        
        #endregion

        #region IQueueWorker<TAssignment> Members

        public void Enqueue(TAssignment assignment)
        {
            this.CheckState2(
                $"'{nameof(Enqueue)}' requested.",
                WorkerState.Starting,
                WorkerState.Running,

                WorkerState.Pausing,
                WorkerState.Paused,

                WorkerState.Resuming);

            lock (_dataLock)
            {
                _assignments.Enqueue(assignment);
                _dataSignal.Set();
            }
        }

        public int Backlog
        {
            get
            {
                lock (_dataLock)
                {
                    return _assignments.Count;
                }
            }
        }

        #endregion
    }
}
