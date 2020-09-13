using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TauCode.Working.Workers
{
    public abstract class LoopWorkerBase : WorkerBase
    {
        #region Constants

        protected const int ControlSignalIndex = 0;
        private const int PauseTimeoutMilliseconds = 10;

        #endregion

        #region Nested

        protected enum WorkFinishReason
        {
            GotControlSignal = 1,
            WorkIsDone,
        }

        protected enum VacationFinishReason
        {
            GotControlSignal = 1,
            VacationTimeElapsed,
            NewWorkArrived,
        }

        #endregion

        #region Fields

        private Task _routineTask;

        private AutoResetEvent _controlSignal;
        private AutoResetEvent _routineSignal;

        private WaitHandle[] _controlSignalWithExtraSignals;

        #endregion

        #region Abstract

        protected abstract Task<WorkFinishReason> DoWorkAsyncImpl(); // todo: cancellation token.

        protected abstract Task<VacationFinishReason> TakeVacationAsyncImpl();

        protected abstract IList<AutoResetEvent> CreateExtraSignals();

        #endregion

        #region Private

        private async Task LoopRoutine()
        {
            string message;

            message = $"'{nameof(LoopRoutine)}' started.";
            //this.LogDebug(message, 3);
            this.GetLogger().Debug(message, nameof(LoopRoutine));

            this.CheckState(message, WorkerState.Starting);

            message = $"Acknowledging control thread that {nameof(LoopRoutine)} is ready to go.";
            //this.LogDebug(message, 3);
            this.GetLogger().Debug(message, nameof(LoopRoutine));

            WaitHandle.SignalAndWait(_routineSignal, _controlSignal);
            this.CheckState(message, WorkerState.Running);

            var goOn = true;

            message = $"Entering '{nameof(LoopRoutine)}' loop.";
            //this.LogDebug(message, 3);
            this.GetLogger().Debug(message, nameof(LoopRoutine));


            while (goOn)
            {
                var workFinishReason = await this.DoWorkAsync();
                message = $"{nameof(DoWorkAsync)} result: {workFinishReason}.";
                //this.LogDebug(, 3);
                this.GetLogger().Debug(message, nameof(LoopRoutine));

                if (workFinishReason == WorkFinishReason.GotControlSignal)
                {
                    goOn = this.ContinueAfterControlSignal(WorkerState.Pausing, WorkerState.Stopping, WorkerState.Disposing);
                }
                else if (workFinishReason == WorkFinishReason.WorkIsDone)
                {
                    var vacationFinishedReason = await this.TakeVacationAsync();
                    message = $"{nameof(TakeVacationAsync)} result: {vacationFinishedReason}.";
                    //this.LogDebug(, 3);
                    this.GetLogger().Debug(message, nameof(LoopRoutine));

                    switch (vacationFinishedReason)
                    {
                        case VacationFinishReason.GotControlSignal:
                            goOn = this.ContinueAfterControlSignal(WorkerState.Pausing, WorkerState.Stopping, WorkerState.Disposing);
                            break;

                        case VacationFinishReason.VacationTimeElapsed:
                        case VacationFinishReason.NewWorkArrived:
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
            this.LogDebug($"Entered.");
            return this.DoWorkAsyncImpl();
        }

        private Task<VacationFinishReason> TakeVacationAsync()
        {
            this.LogDebug($"Entered.");
            return this.TakeVacationAsyncImpl();
        }

        private void PauseRoutine()
        {
            this.LogDebug($"Entered.");

            while (true)
            {
                var gotControlSignal = _controlSignal.WaitOne(PauseTimeoutMilliseconds);
                if (gotControlSignal)
                {
                    this.LogDebug("Got control signal.");
                    return;
                }
            }
        }

        private bool ContinueAfterControlSignal(params WorkerState[] expectedStates)
        {
            var message = "Continuing after control signal.";
            this.LogDebug(message);
            this.CheckState(message, expectedStates);

            this.LogDebug("Sending signal to control thread and awaiting response signal.");
            WaitHandle.SignalAndWait(_routineSignal, _controlSignal);

            message = "Got response signal from control thread.";
            this.LogDebug(message);
            var stableStates = expectedStates
                .Select(WorkingExtensions.GetStableWorkerState)
                .ToArray();

            this.CheckState(message, stableStates);

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

        protected int WaitForControlSignalWithExtraSignals(int millisecondsTimeout) =>
            this.WaitForControlSignalWithExtraSignals(TimeSpan.FromMilliseconds(millisecondsTimeout));

        protected int WaitForControlSignalWithExtraSignals(TimeSpan timeout)
        {
            var index = WaitHandle.WaitAny(_controlSignalWithExtraSignals, timeout);
            return index;
        }

        protected virtual void Shutdown(WorkerState shutdownState)
        {
            this.LogDebug($"Sending signal to {nameof(LoopRoutine)}.");
            WaitHandle.SignalAndWait(_controlSignal, _routineSignal);

            this.ChangeState(shutdownState);
            _controlSignal.Set();

            this.LogDebug($"Waiting {nameof(LoopRoutine)} to terminate.");
            this._routineTask.Wait();
            this.LogDebug($"{nameof(LoopRoutine)} terminated.");

            this._routineTask.Dispose();
            this._routineTask = null;

            foreach (var signal in _controlSignalWithExtraSignals)
            {
                signal.Dispose();
            }

            _controlSignalWithExtraSignals = null;

            _controlSignal = null;

            _routineSignal.Dispose();
            _routineSignal = null;

            this.LogDebug("OS Resources disposed.");
        }

        #endregion

        #region Overridden

        protected override void StartImpl()
        {
            this.ChangeState(WorkerState.Starting);

            _controlSignal = new AutoResetEvent(false);
            _routineSignal = new AutoResetEvent(false);

            var controlSignalWithExtraSignalsList = new List<AutoResetEvent>
            {
                _controlSignal, // always has index #0
            };

            var extraSignals = this.CreateExtraSignals();

            var extraSignalsOk =
                extraSignals != null &&
                extraSignals.Distinct().Count() == extraSignals.Count &&
                extraSignals.All(x => x != null);

            if (!extraSignalsOk)
            {
                throw new InvalidOperationException($"'{nameof(CreateExtraSignals)}' must return non-null list with unique non-null elements.");
            }
            
            controlSignalWithExtraSignalsList.AddRange(extraSignals);
            _controlSignalWithExtraSignals = controlSignalWithExtraSignalsList
                .Cast<WaitHandle>()
                .ToArray();


            this._routineTask = Task.Factory.StartNew(this.LoopRoutine);

            // wait signal from routine that routine has started
            _routineSignal.WaitOne();

            this.ChangeState(WorkerState.Running);

            // inform routine that state has been changed to 'Running' and routine can start actual work
            _controlSignal.Set();
        }

        protected override void PauseImpl()
        {
            this.ChangeState(WorkerState.Pausing);

            WaitHandle.SignalAndWait(_controlSignal, _routineSignal);

            this.ChangeState(WorkerState.Paused);
            _controlSignal.Set();
        }

        protected override void ResumeImpl()
        {
            this.ChangeState(WorkerState.Resuming);

            WaitHandle.SignalAndWait(_controlSignal, _routineSignal);

            this.ChangeState(WorkerState.Running);
            _controlSignal.Set();
        }

        protected override void StopImpl()
        {
            this.ChangeState(WorkerState.Stopping);

            this.Shutdown(WorkerState.Stopped);
        }

        protected override void DisposeImpl()
        {
            var previousState = this.State;
            this.ChangeState(WorkerState.Disposing);

            if (previousState == WorkerState.Stopped)
            {
                this.LogDebug("Worker was stopped, nothing to dispose.");
                this.ChangeState(WorkerState.Disposed);
                return;
            }

            this.Shutdown(WorkerState.Disposed);
        }

        #endregion
    }
}
