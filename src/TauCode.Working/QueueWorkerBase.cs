using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TauCode.Working.Lab
{
    public abstract class QueueWorkerBase<TAssignment> : WorkerBase, IQueueWorker<TAssignment>
    {
        #region Nested

        private enum NoProcessingReason
        {
            NoAssignments = 1,
            GotControlSignal,
        }

        private enum IdleStateInterruptionReason
        {
            GotControlSignal = 1,
            GotAssignment,
        }

        #endregion

        #region Constants

        private const int Timeout = 1; // 1 ms
        private const int ControlSignalIndex = 0;
        private const int DataSignalIndex = 1;

        #endregion

        #region Fields

        private readonly Queue<TAssignment> _assignments;
        private readonly object _dataLock;

        private AutoResetEvent _controlSignal;
        private AutoResetEvent _dataSignal;
        private AutoResetEvent _controlRequestAcknowledgedSignal;
        private WaitHandle[] _handles;
        private Task _task;

        #endregion

        #region Constructor

        protected QueueWorkerBase()
        {
            _assignments = new Queue<TAssignment>();
            _dataLock = new object();
        }

        #endregion

        #region Abstract

        protected abstract void DoAssignment(TAssignment assignment);

        #endregion

        #region Overridden

        protected override void StartImpl()
        {
            this.ChangeState(WorkerState.Starting);

            _controlSignal = new AutoResetEvent(false);
            _dataSignal = new AutoResetEvent(false);
            _controlRequestAcknowledgedSignal = new AutoResetEvent(false);

            _handles = new WaitHandle[] { _controlSignal, _dataSignal };

            this.LogVerbose("Creating task");

            _task = new Task(this.Routine);
            _task.Start();

            this.LogVerbose("Task started");
            _controlRequestAcknowledgedSignal.WaitOne();
            this.LogVerbose("Got acknowledge signal from routine");
            this.ChangeState(WorkerState.Running);
            _controlSignal.Set();
        }

        protected override void PauseImpl()
        {
            this.LogVerbose("Pause requested");
            this.ChangeState(WorkerState.Pausing);
            _controlSignal.Set();
            _controlRequestAcknowledgedSignal.WaitOne();
            this.ChangeState(WorkerState.Paused);
            _controlSignal.Set();
        }

        protected override void ResumeImpl()
        {
            this.LogVerbose("Resume requested");
            this.ChangeState(WorkerState.Resuming);
            _controlSignal.Set();
            _controlRequestAcknowledgedSignal.WaitOne();
            this.ChangeState(WorkerState.Running);
            _controlSignal.Set();
        }

        protected override void StopImpl()
        {
            this.LogVerbose("Stop requested");
            this.ChangeState(WorkerState.Stopping);
            _controlSignal.Set();
            _controlRequestAcknowledgedSignal.WaitOne();
            this.ChangeState(WorkerState.Stopped);
            _controlSignal.Set();

            this.LogVerbose("Waiting task to terminate.");
            _task.Wait();
            this.LogVerbose("Task terminated.");

            _task.Dispose();
            _task = null;

            _controlSignal.Dispose();
            _controlSignal = null;

            _dataSignal.Dispose();
            _dataSignal = null;

            _controlRequestAcknowledgedSignal.Dispose();
            _controlRequestAcknowledgedSignal = null;

            _handles = null;

            this.LogVerbose("OS Resources disposed.");
        }

        protected override void DisposeImpl()
        {
            this.LogVerbose("Dispose requested");
            var previousState = this.State;
            this.ChangeState(WorkerState.Disposing);

            if (previousState == WorkerState.Stopped)
            {
                this.LogVerbose("Worker was stopped, nothing to dispose");
                this.ChangeState(WorkerState.Disposed);
                return;
            }

            _controlSignal.Set();
            _controlRequestAcknowledgedSignal.WaitOne();
            this.ChangeState(WorkerState.Disposed);
            _controlSignal.Set();

            this.LogVerbose("Waiting task to terminate.");
            _task.Wait();
            this.LogVerbose("Task terminated.");

            _task.Dispose();
            _task = null;

            _controlSignal.Dispose();
            _controlSignal = null;

            _dataSignal.Dispose();
            _dataSignal = null;

            _controlRequestAcknowledgedSignal.Dispose();
            _controlRequestAcknowledgedSignal = null;

            _handles = null;

            this.LogVerbose("OS Resources disposed.");
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

        private NoProcessingReason ProcessAssignments()
        {
            NoProcessingReason reason;

            while (true)
            {
                var gotControlSignal = _controlSignal.WaitOne(0);
                if (gotControlSignal)
                {
                    reason = NoProcessingReason.GotControlSignal;
                    break;
                }

                var gotAssignment = this.TryGetAssignment(out var assignment);
                if (gotAssignment)
                {
                    try
                    {
                        this.DoAssignment(assignment);
                    }
                    catch (Exception ex)
                    {
                        LogError($"Assignment {assignment} caused an exception: {ex}.");
                    }
                }
                else
                {
                    reason = NoProcessingReason.NoAssignments;
                    break;
                }
            }

            return reason;
        }

        private void Routine()
        {
            this.CheckState(WorkerState.Starting);

            _controlRequestAcknowledgedSignal.Set(); // inform control thread that routine has started.
            _controlSignal.WaitOne();

            this.LogVerbose("Got control signal from control thread");
            this.CheckState(WorkerState.Running);

            var goOn = true;

            while (goOn)
            {
                var reason = this.ProcessAssignments();

                if (reason == NoProcessingReason.GotControlSignal)
                {
                    this.CheckState(WorkerState.Pausing, WorkerState.Stopping, WorkerState.Disposing);

                    _controlRequestAcknowledgedSignal.Set();
                    _controlSignal.WaitOne();

                    var state = this.State;

                    this.CheckState(WorkerState.Paused, WorkerState.Stopped, WorkerState.Disposed);

                    switch (state)
                    {
                        case WorkerState.Disposed:
                        case WorkerState.Stopped:
                            goOn = false;
                            break;

                        case WorkerState.Paused:
                            this.PauseRoutine();
                            state = this.State;
                            if (state == WorkerState.Stopped || state == WorkerState.Disposed)
                            {
                                goOn = false;
                            }
                            else
                            {
                                // simply go on.
                            }
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else if (reason == NoProcessingReason.NoAssignments)
                {
                    var interruptionReason = this.IdleRoutine();

                    switch (interruptionReason)
                    {
                        case IdleStateInterruptionReason.GotControlSignal:
                            this.CheckState(WorkerState.Stopped, WorkerState.Paused, WorkerState.Disposed);
                            var state = this.State;
                            if (state == WorkerState.Stopped || state == WorkerState.Disposed)
                            {
                                goOn = false;
                            }
                            else
                            {
                                // state is 'Paused'
                                this.PauseRoutine();
                                state = this.State;
                                if (state == WorkerState.Stopped || state == WorkerState.Disposed)
                                {
                                    goOn = false;
                                }
                                else
                                {
                                    // simply go on.
                                }
                            }

                            break;

                        case IdleStateInterruptionReason.GotAssignment:
                            // nice, got some data
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    throw new WorkingException("Internal error."); // should never happen
                }
            }

            this.LogVerbose($"Exiting task. State is '{this.State}'.");
        }

        private IdleStateInterruptionReason IdleRoutine()
        {
            this.LogVerbose("Entered idle routine");

            while (true)
            {
                var signalIndex = WaitHandle.WaitAny(_handles, Timeout);
                switch (signalIndex)
                {
                    case ControlSignalIndex:
                        this.LogVerbose("Got control signal");
                        this.CheckState(WorkerState.Stopping, WorkerState.Pausing, WorkerState.Disposing);
                        _controlRequestAcknowledgedSignal.Set();
                        _controlSignal.WaitOne();
                        this.CheckState(WorkerState.Stopped, WorkerState.Paused, WorkerState.Disposed);
                        return IdleStateInterruptionReason.GotControlSignal;

                    case DataSignalIndex:
                        this.LogVerbose("Got data");
                        return IdleStateInterruptionReason.GotAssignment;
                }
            }
        }

        private void PauseRoutine()
        {
            this.LogVerbose("Entered pause routine");

            while (true)
            {
                var gotControlSignal = _controlSignal.WaitOne(Timeout);
                if (gotControlSignal)
                {
                    this.LogVerbose("Got control signal");
                    this.CheckState(WorkerState.Stopping, WorkerState.Resuming, WorkerState.Disposing);
                    _controlRequestAcknowledgedSignal.Set();
                    _controlSignal.WaitOne();
                    this.CheckState(WorkerState.Stopped, WorkerState.Running, WorkerState.Disposed);
                    return;
                }
            }
        }

        #endregion

        #region IQueueWorker<TAssignment> Members

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

        #endregion
    }
}
