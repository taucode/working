﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// todo clean up
namespace TauCode.Working
{
    public abstract class LoopWorkerBase : WorkerBase
    {
        #region Nested

        protected enum WorkFinishReason
        {
            GotControlSignal = 1,
            WorkIsDone,
        }

        protected enum VacationFinishedReason
        {
            GotControlSignal = 1,
            VacationTimeElapsed,
            NewWorkArrived,
        }

        #endregion

        #region Fields

        private AutoResetEvent _controlSignal;
        private AutoResetEvent _routineSignal;

        #endregion

        #region Abstract

        protected abstract Task<WorkFinishReason> DoWorkAsync();

        protected abstract VacationFinishedReason TakeVacation();

        protected abstract AutoResetEvent[] GetExtraSignals();

        #endregion

        #region Protected

        //protected AutoResetEvent ControlSignal { get; private set; }
        //protected AutoResetEvent RoutineSignal { get; private set; }
        protected Task LoopTask { get; private set; } // todo: private?

        protected bool WaitControlSignal(int millisecondsTimeout)
        {
            return _controlSignal.WaitOne(millisecondsTimeout);
        }

        //protected void RegisterAdditionalHandles(WaitHandle[] additionalHandles)
        //{
        //    throw new NotImplementedException();
        //}

        //protected void DeregisterAdditionalHandles()
        //{
        //    throw new NotImplementedException();
        //}

        protected int WaitForControlSignalWithExtraSignals(int ms) // todo rename
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Overridden

        protected override void StartImpl()
        {
            this.ChangeState(WorkerState.Starting);

            _controlSignal = new AutoResetEvent(false);
            _routineSignal = new AutoResetEvent(false);

            //this.LoopTask = new Task(this.Routine);
            this.LoopTask = Task.Factory.StartNew(this.Routine);
            this.LoopTask.Start();

            // wait signal from routine that routine has started
            _routineSignal.WaitOne();

            this.ChangeState(WorkerState.Running);

            // inform routine that state has been changed to 'Running' and routine cat start actual work
            _controlSignal.Set();
        }

        protected override void PauseImpl()
        {
            this.LogVerbose("Pause requested");
            this.ChangeState(WorkerState.Pausing);
            _controlSignal.Set();
            _routineSignal.WaitOne();
            this.ChangeState(WorkerState.Paused);
            _controlSignal.Set();

        }

        protected override void ResumeImpl()
        {
            this.LogVerbose("Resume requested");
            this.ChangeState(WorkerState.Resuming);
            _controlSignal.Set();
            _routineSignal.WaitOne();
            this.ChangeState(WorkerState.Running);
            _controlSignal.Set();
        }

        protected override void StopImpl()
        {
            this.LogVerbose("Stop requested");
            this.ChangeState(WorkerState.Stopping);
            _controlSignal.Set();
            _routineSignal.WaitOne();
            this.ChangeState(WorkerState.Stopped);
            _controlSignal.Set();

            this.LogVerbose("Waiting task to terminate.");
            this.LoopTask.Wait();
            this.LogVerbose("Task terminated.");

            this.LoopTask.Dispose();
            this.LoopTask = null;

            _controlSignal.Dispose();
            _controlSignal = null;

            //_dataSignal.Dispose();
            //_dataSignal = null;

            _routineSignal.Dispose();
            _routineSignal = null;

            //_handles = null;

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
            _routineSignal.WaitOne();
            this.ChangeState(WorkerState.Disposed);
            _controlSignal.Set();

            this.LogVerbose("Waiting task to terminate.");
            this.LoopTask.Wait();
            this.LogVerbose("Task terminated.");

            this.LoopTask.Dispose();
            this.LoopTask = null;

            _controlSignal.Dispose();
            _controlSignal = null;

            //_dataSignal.Dispose();
            //_dataSignal = null;

            _routineSignal.Dispose();
            _routineSignal = null;

            //_handles = null;

            this.LogVerbose("OS Resources disposed.");
        }

        #endregion

        private async Task Routine()
        {
            this.CheckState(WorkerState.Starting);

            _routineSignal.Set();

            _controlSignal.WaitOne();
            this.CheckState(WorkerState.Running);

            var goOn = true;

            while (goOn)
            {
                var workFinishReason = await this.DoWorkAsync();

                if (workFinishReason == WorkFinishReason.GotControlSignal)
                {
                    goOn = this.ContinueAfterControlSignal(WorkerState.Pausing, WorkerState.Stopping, WorkerState.Disposing);

                    //this.CheckState(WorkerState.Pausing, WorkerState.Stopping, WorkerState.Disposing);

                    //this.RoutineSignal.Set();
                    //this.ControlSignal.WaitOne();

                    //var state = this.State;

                    //this.CheckState(WorkerState.Paused, WorkerState.Stopped, WorkerState.Disposed);
                    //switch (state)
                    //{
                    //    case WorkerState.Disposed:
                    //    case WorkerState.Stopped:
                    //        goOn = false;
                    //        break;

                    //    case WorkerState.Paused:
                    //        this.PauseRoutine();
                    //        state = this.State;
                    //        if (state == WorkerState.Stopped || state == WorkerState.Disposed)
                    //        {
                    //            goOn = false;
                    //        }
                    //        else
                    //        {
                    //            this.CheckState(WorkerState.Running);
                    //            // simply go on.
                    //        }
                    //        break;

                    //    default:
                    //        throw new WorkingException("Internal error."); // should never happen
                    //}
                }
                else if (workFinishReason == WorkFinishReason.WorkIsDone)
                {
                    var vacationFinishedReason = this.TakeVacation();

                    switch (vacationFinishedReason)
                    {
                        case VacationFinishedReason.GotControlSignal:
                            goOn = this.ContinueAfterControlSignal(WorkerState.Pausing, WorkerState.Stopping, WorkerState.Disposing);

                            //this.CheckState(WorkerState.Stopped, WorkerState.Paused, WorkerState.Disposed);
                            //var state = this.State;
                            //if (state == WorkerState.Stopped || state == WorkerState.Disposed)
                            //{
                            //    goOn = false;
                            //}
                            //else
                            //{
                            //    // state is 'Paused'
                            //    this.PauseRoutine();
                            //    state = this.State;
                            //    if (state == WorkerState.Stopped || state == WorkerState.Disposed)
                            //    {
                            //        goOn = false;
                            //    }
                            //    else
                            //    {
                            //        // simply go on.
                            //    }
                            //}

                            break;

                        case VacationFinishedReason.VacationTimeElapsed:
                        case VacationFinishedReason.NewWorkArrived:
                            // let's get back to work.
                            break;

                        default:
                            throw new WorkingException("Internal error."); // should never happen
                    }
                }
                else
                {
                    throw new WorkingException("Internal error."); // should never happen
                }
            }
        }

        private void PauseRoutine()
        {
            this.LogVerbose("Entered pause routine");

            while (true)
            {
                var gotControlSignal = _controlSignal.WaitOne(11); // todo
                if (gotControlSignal)
                {
                    //this.LogVerbose("Got control signal");

                    //this.CheckState(WorkerState.Stopping, WorkerState.Resuming, WorkerState.Disposing);

                    //this.RoutineSignal.Set();
                    //this.ControlSignal.WaitOne();

                    //this.CheckState(WorkerState.Stopped, WorkerState.Running, WorkerState.Disposed);
                    return;
                }
            }
        }

        private bool ContinueAfterControlSignal(params WorkerState[] expectedStates)
        {
            //if (expectedStates.Length == 0)
            //{
            //    throw new NotImplementedException(); // todo
            //}

            //if (!expectedStates.All(x => x.IsTransitionWorkerState()))
            //{
            //    throw new NotImplementedException(); // todo
            //}

            //this.CheckState(WorkerState.Pausing, WorkerState.Stopping, WorkerState.Disposing);
            this.CheckState(expectedStates);

            _routineSignal.Set();
            _controlSignal.WaitOne();

            var stableStates = expectedStates
                .Select(WorkingExtensions.GetStableWorkerState)
                .ToArray();

            this.CheckState(stableStates);

            var state = this.State;

            bool result;

            switch (state)
            {
                case WorkerState.Disposed:
                case WorkerState.Stopped:
                    result = false;
                    break;

                case WorkerState.Paused:
                    this.PauseRoutine();
                    //state = this.State;
                    //if (state == WorkerState.Stopped || state == WorkerState.Disposed)
                    //{
                    //    result = false;
                    //}
                    //else
                    //{
                    //    this.CheckState(WorkerState.Running);
                    //    result = true;
                    //}

                    // After exit from 'PauseRoutine()', state cannot be 'Pausing', therefore recursion is never endless.
                    result = ContinueAfterControlSignal(WorkerState.Stopping, WorkerState.Resuming, WorkerState.Disposing);
                    break;

                case WorkerState.Running:
                    result = true;
                    break;

                default:
                    throw new WorkingException("Internal error."); // should never happen
            }

            return result;
        }
    }
}
