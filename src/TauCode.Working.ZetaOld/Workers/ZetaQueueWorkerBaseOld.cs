﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Working.Exceptions;

namespace TauCode.Working.ZetaOld.Workers
{
    // todo remove
    public abstract class ZetaQueueWorkerBaseOld<TAssignment> : ZetaWorkerBase, IZetaQueueWorker<TAssignment>
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

        protected ZetaQueueWorkerBaseOld()
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
            this.ChangeState(ZetaWorkerState.Starting);

            _controlSignal = new AutoResetEvent(false);
            _dataSignal = new AutoResetEvent(false);
            _controlRequestAcknowledgedSignal = new AutoResetEvent(false);

            _handles = new WaitHandle[] { _controlSignal, _dataSignal };

            //this.LogDebug("Creating task");

            _task = new Task(this.Routine);
            _task.Start();

            //this.LogDebug("Task started");
            _controlRequestAcknowledgedSignal.WaitOne();
            //this.LogDebug("Got acknowledge signal from routine");
            this.ChangeState(ZetaWorkerState.Running);
            _controlSignal.Set();
        }

        protected override void PauseImpl()
        {   
            this.ChangeState(ZetaWorkerState.Pausing);
            _controlSignal.Set();
            _controlRequestAcknowledgedSignal.WaitOne();
            this.ChangeState(ZetaWorkerState.Paused);
            _controlSignal.Set();
        }

        protected override void ResumeImpl()
        {
            this.ChangeState(ZetaWorkerState.Resuming);
            _controlSignal.Set();
            _controlRequestAcknowledgedSignal.WaitOne();
            this.ChangeState(ZetaWorkerState.Running);
            _controlSignal.Set();
        }

        protected override void StopImpl()
        {
            this.ChangeState(ZetaWorkerState.Stopping);
            _controlSignal.Set();
            _controlRequestAcknowledgedSignal.WaitOne();
            this.ChangeState(ZetaWorkerState.Stopped);
            _controlSignal.Set();

            //this.LogDebug("Waiting task to terminate.");
            _task.Wait();
            //this.LogDebug("Task terminated.");

            _task.Dispose();
            _task = null;

            _controlSignal.Dispose();
            _controlSignal = null;

            _dataSignal.Dispose();
            _dataSignal = null;

            _controlRequestAcknowledgedSignal.Dispose();
            _controlRequestAcknowledgedSignal = null;

            _handles = null;

            //this.LogDebug("OS Resources disposed.");
        }

        protected override void DisposeImpl()
        {
            var previousState = this.State;
            this.ChangeState(ZetaWorkerState.Disposing);

            if (previousState == ZetaWorkerState.Stopped)
            {
                //this.LogDebug("Worker was stopped, nothing to dispose");
                this.ChangeState(ZetaWorkerState.Disposed);
                return;
            }

            _controlSignal.Set();
            _controlRequestAcknowledgedSignal.WaitOne();
            this.ChangeState(ZetaWorkerState.Disposed);
            _controlSignal.Set();

            //this.LogDebug("Waiting task to terminate.");
            _task.Wait();
            //this.LogDebug("Task terminated.");

            _task.Dispose();
            _task = null;

            _controlSignal.Dispose();
            _controlSignal = null;

            _dataSignal.Dispose();
            _dataSignal = null;

            _controlRequestAcknowledgedSignal.Dispose();
            _controlRequestAcknowledgedSignal = null;

            _handles = null;

            //this.LogDebug("OS Resources disposed.");
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
                        //LogError($"Assignment {assignment} caused an exception: {ex}.");
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
            this.CheckState("todo", ZetaWorkerState.Starting);

            _controlRequestAcknowledgedSignal.Set(); // inform control thread that routine has started.
            _controlSignal.WaitOne();

            //this.LogDebug("Got control signal from control thread");
            this.CheckState("todo", ZetaWorkerState.Running);

            var goOn = true;

            while (goOn)
            {
                var reason = this.ProcessAssignments();

                if (reason == NoProcessingReason.GotControlSignal)
                {
                    this.CheckState("todo", ZetaWorkerState.Pausing, ZetaWorkerState.Stopping, ZetaWorkerState.Disposing);

                    _controlRequestAcknowledgedSignal.Set();
                    _controlSignal.WaitOne();

                    var state = this.State;

                    this.CheckState("todo", ZetaWorkerState.Paused, ZetaWorkerState.Stopped, ZetaWorkerState.Disposed);

                    switch (state)
                    {
                        case ZetaWorkerState.Disposed:
                        case ZetaWorkerState.Stopped:
                            goOn = false;
                            break;

                        case ZetaWorkerState.Paused:
                            this.PauseRoutine();
                            state = this.State;
                            if (state == ZetaWorkerState.Stopped || state == ZetaWorkerState.Disposed)
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
                            this.CheckState("todo", ZetaWorkerState.Stopped, ZetaWorkerState.Paused, ZetaWorkerState.Disposed);
                            var state = this.State;
                            if (state == ZetaWorkerState.Stopped || state == ZetaWorkerState.Disposed)
                            {
                                goOn = false;
                            }
                            else
                            {
                                // state is 'Paused'
                                this.PauseRoutine();
                                state = this.State;
                                if (state == ZetaWorkerState.Stopped || state == ZetaWorkerState.Disposed)
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

            //this.LogDebug($"Exiting task. State is '{this.State}'.");
        }

        private IdleStateInterruptionReason IdleRoutine()
        {
            //this.LogDebug("Entered idle routine");

            while (true)
            {
                var signalIndex = WaitHandle.WaitAny(_handles, Timeout);
                switch (signalIndex)
                {
                    case ControlSignalIndex:
                        //this.LogDebug("Got control signal");
                        this.CheckState("todo", ZetaWorkerState.Stopping, ZetaWorkerState.Pausing, ZetaWorkerState.Disposing);
                        _controlRequestAcknowledgedSignal.Set();
                        _controlSignal.WaitOne();
                        this.CheckState("todo", ZetaWorkerState.Stopped, ZetaWorkerState.Paused, ZetaWorkerState.Disposed);
                        return IdleStateInterruptionReason.GotControlSignal;

                    case DataSignalIndex:
                        //this.LogDebug("Got data");
                        return IdleStateInterruptionReason.GotAssignment;
                }
            }
        }

        private void PauseRoutine()
        {
            //this.LogDebug("Entered pause routine");

            while (true)
            {
                var gotControlSignal = _controlSignal.WaitOne(Timeout);
                if (gotControlSignal)
                {
                    //this.LogDebug("Got control signal");
                    this.CheckState("todo", ZetaWorkerState.Stopping, ZetaWorkerState.Resuming, ZetaWorkerState.Disposing);
                    _controlRequestAcknowledgedSignal.Set();
                    _controlSignal.WaitOne();
                    this.CheckState("todo", ZetaWorkerState.Stopped, ZetaWorkerState.Running, ZetaWorkerState.Disposed);
                    return;
                }
            }
        }

        #endregion

        #region IQueueWorker<TAssignment> Members

        public void Enqueue(TAssignment assignment)
        {
            this.CheckState(
                "todo",
                ZetaWorkerState.Starting,
                ZetaWorkerState.Running,

                ZetaWorkerState.Pausing,
                ZetaWorkerState.Paused,

                ZetaWorkerState.Resuming);

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