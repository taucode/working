using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// todo clean up
namespace TauCode.Working
{
    public abstract class LoopWorkerBase : WorkerBase
    {
        #region Constants

        protected const int ControlSignalIndex = 0;

        #endregion

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

        private WaitHandle[] _controlSignalWithExtraSignals;

        #endregion

        #region Abstract

        protected abstract Task<WorkFinishReason> DoWorkAsyncImpl();

        protected abstract Task<VacationFinishedReason> TakeVacationAsyncImpl();

        protected abstract AutoResetEvent[] GetExtraSignals();

        #endregion

        #region Private

        private async Task Routine()
        {
            this.LogDebug("Routine started", 3);

            this.CheckState(WorkerState.Starting);
            WaitHandle.SignalAndWait(_routineSignal, _controlSignal);

            //_routineSignal.Set();
            //_controlSignal.WaitOne();

            this.CheckState(WorkerState.Running);

            var goOn = true;

            while (goOn)
            {
                var workFinishReason = await this.DoWorkAsync();
                this.LogDebug($"{nameof(DoWorkAsync)} result: {workFinishReason}", 3);

                if (workFinishReason == WorkFinishReason.GotControlSignal)
                {
                    goOn = this.ContinueAfterControlSignal(WorkerState.Pausing, WorkerState.Stopping, WorkerState.Disposing);
                }
                else if (workFinishReason == WorkFinishReason.WorkIsDone)
                {
                    var vacationFinishedReason = await this.TakeVacationAsync();
                    this.LogDebug($"{nameof(TakeVacationAsync)} result: {vacationFinishedReason}", 3);

                    switch (vacationFinishedReason)
                    {
                        case VacationFinishedReason.GotControlSignal:
                            goOn = this.ContinueAfterControlSignal(WorkerState.Pausing, WorkerState.Stopping, WorkerState.Disposing);
                            break;

                        case VacationFinishedReason.VacationTimeElapsed:
                        case VacationFinishedReason.NewWorkArrived:
                            // let's get back to work.
                            break;

                        default:
                            throw this.CreateInternalErrorException(); // should never happen
                    }
                }
                else
                {
                    throw this.CreateInternalErrorException(); // should never happen
                }
            }
        }

        private Task<WorkFinishReason> DoWorkAsync()
        {
            this.LogDebug($"Entered");
            return this.DoWorkAsyncImpl();
        }

        private Task<VacationFinishedReason> TakeVacationAsync()
        {
            this.LogDebug($"Entered {nameof(TakeVacationAsync)}");
            return this.TakeVacationAsyncImpl();
        }

        private void PauseRoutine()
        {
            this.LogDebug($"Entered {nameof(PauseRoutine)}");

            while (true)
            {
                var gotControlSignal = _controlSignal.WaitOne(11); // todo
                if (gotControlSignal)
                {
                    this.LogDebug("Got control signal");

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
                    throw this.CreateInternalErrorException(); // should never happen
            }

            return result;
        }

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

        protected int WaitForControlSignalWithExtraSignals(int millisecondsTimeout) =>
            this.WaitForControlSignalWithExtraSignals(TimeSpan.FromMilliseconds(millisecondsTimeout));
        //{
        //    if (_controlSignalWithExtraSignals == null)
        //    {
        //        throw new InvalidOperationException(); // todo
        //    }

        //    Task todo;
        //    var index = WaitHandle.WaitAny(_controlSignalWithExtraSignals, millisecondsTimeout);
        //    return index;
        //}

        protected int WaitForControlSignalWithExtraSignals(TimeSpan timeout) // todo rename
        {
            if (_controlSignalWithExtraSignals == null)
            {
                throw new InvalidOperationException(); // todo
            }

            var index = WaitHandle.WaitAny(_controlSignalWithExtraSignals, timeout);
            return index;
        }

        #endregion

        #region Overridden

        protected override void StartImpl()
        {
            this.ChangeState(WorkerState.Starting);

            _controlSignal = new AutoResetEvent(false);
            _routineSignal = new AutoResetEvent(false);

            var extraSignals = this.GetExtraSignals();
            if (extraSignals == null)
            {
                this.CheckInternalIntegrity(_controlSignalWithExtraSignals == null);
            }
            else
            {
                if (extraSignals.Length == 0)
                {
                    throw new NotImplementedException(); // todo. if you don't need extra signals, return null instead of empty array.
                }

                var distinctExtraSignals = extraSignals.Distinct().ToArray();
                if (extraSignals.Length != distinctExtraSignals.Length)
                {
                    throw new NotImplementedException(); // must be different.
                }

                var list = new List<WaitHandle>();
                list.Add(_controlSignal); // always has index #0
                list.AddRange(distinctExtraSignals);

                _controlSignalWithExtraSignals = list.ToArray();
            }

            //this.LoopTask = new Task(this.Routine);
            this.LoopTask = Task.Factory.StartNew(this.Routine);
            //this.LoopTask.Start();

            // wait signal from routine that routine has started
            _routineSignal.WaitOne();

            this.ChangeState(WorkerState.Running);

            // inform routine that state has been changed to 'Running' and routine cat start actual work
            _controlSignal.Set();
        }

        protected override void PauseImpl()
        {
            this.LogDebug("Pause requested");
            this.ChangeState(WorkerState.Pausing);


            _controlSignal.Set();
            _routineSignal.WaitOne();

            this.ChangeState(WorkerState.Paused);
            _controlSignal.Set();
        }

        protected override void ResumeImpl()
        {
            this.LogDebug("Resume requested");
            this.ChangeState(WorkerState.Resuming);
            _controlSignal.Set();
            _routineSignal.WaitOne();
            this.ChangeState(WorkerState.Running);
            _controlSignal.Set();
        }

        protected override void StopImpl()
        {
            this.LogDebug("Stop requested");
            this.ChangeState(WorkerState.Stopping);
            _controlSignal.Set();
            _routineSignal.WaitOne();
            this.ChangeState(WorkerState.Stopped);
            _controlSignal.Set();

            this.LogDebug("Waiting task to terminate.");
            this.LoopTask.Wait();
            this.LogDebug("Task terminated.");

            this.LoopTask.Dispose();
            this.LoopTask = null;

            _controlSignal.Dispose();
            _controlSignal = null;

            //_dataSignal.Dispose();
            //_dataSignal = null;

            _routineSignal.Dispose();
            _routineSignal = null;

            //_handles = null;

            this.LogDebug("OS Resources disposed.");
        }

        protected override void DisposeImpl()
        {
            this.LogDebug("Dispose requested");
            var previousState = this.State;
            this.ChangeState(WorkerState.Disposing);

            if (previousState == WorkerState.Stopped)
            {
                this.LogDebug("Worker was stopped, nothing to dispose");
                this.ChangeState(WorkerState.Disposed);
                return;
            }

            _controlSignal.Set();
            _routineSignal.WaitOne();
            this.ChangeState(WorkerState.Disposed);
            _controlSignal.Set();

            this.LogDebug("Waiting task to terminate.");
            this.LoopTask.Wait();
            this.LogDebug("Task terminated.");

            this.LoopTask.Dispose();
            this.LoopTask = null;

            _controlSignal.Dispose();
            _controlSignal = null;

            //_dataSignal.Dispose();
            //_dataSignal = null;

            _routineSignal.Dispose();
            _routineSignal = null;

            //_handles = null;

            this.LogDebug("OS Resources disposed.");
        }

        #endregion

    }
}
