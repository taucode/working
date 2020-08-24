using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TauCode.Working
{
    public abstract class QueueWorkerBase2<TAssignment> : LoopWorkerBase, IQueueWorker<TAssignment>
    {
        private readonly Queue<TAssignment> _assignments;
        private readonly object _dataLock;
        private AutoResetEvent _dataSignal;

        protected QueueWorkerBase2()
        {
            _assignments = new Queue<TAssignment>();
            _dataLock = new object();

        }

        protected override void StartImpl()
        {
            _dataSignal = new AutoResetEvent(false);
            base.StartImpl();

            throw new NotImplementedException(); // todo: WillWaitForControlSignalWithOthers();
        }

        protected abstract Task DoAssignmentAsync(TAssignment assignment);

        protected override async Task<WorkFinishReason> DoWorkAsync()
        {
            WorkFinishReason reason;

            while (true)
            {
                //var gotControlSignal = _controlSignal.WaitOne(0);
                var gotControlSignal = this.WaitControlSignal(0);

                if (gotControlSignal)
                {
                    reason = WorkFinishReason.GotControlSignal;
                    break;
                }

                var gotAssignment = this.TryGetAssignment(out var assignment);
                if (gotAssignment)
                {
                    try
                    {
                        await this.DoAssignmentAsync(assignment);
                    }
                    catch (Exception ex)
                    {
                        LogError($"Assignment {assignment} caused an exception: {ex}.");
                    }
                }
                else
                {
                    reason = WorkFinishReason.WorkIsDone;
                    break;
                }
            }

            return reason;
        }

        protected override VacationFinishedReason TakeVacation()
        {
            this.LogVerbose("Entered idle routine");

            throw new NotImplementedException(); // continue here.

            //while (true)
            //{
            //    var signalIndex = WaitHandle.WaitAny(_handles, Timeout);
            //    switch (signalIndex)
            //    {
            //        case ControlSignalIndex:
            //            this.LogVerbose("Got control signal");
            //            this.CheckState(WorkerState.Stopping, WorkerState.Pausing, WorkerState.Disposing);
            //            _controlRequestAcknowledgedSignal.Set();
            //            _controlSignal.WaitOne();
            //            this.CheckState(WorkerState.Stopped, WorkerState.Paused, WorkerState.Disposed);
            //            return IdleStateInterruptionReason.GotControlSignal;

            //        case DataSignalIndex:
            //            this.LogVerbose("Got data");
            //            return IdleStateInterruptionReason.GotAssignment;
            //    }
            //}

        }

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

        public void Enqueue(TAssignment assignment)
        {
            this.CheckStateForOperation(
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
    }
}
