using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace TauCode.Working
{
    // todo clean
    public abstract class WorkerBase : IWorker
    {
        #region Fields

        private string _name;
        private WorkerState _state;
        private readonly object _stateLock;
        private readonly object _controlLock;

        private readonly Dictionary<WorkerState, AutoResetEvent> _stateSignals;

        #endregion

        #region Constructor

        protected WorkerBase()
        {
            _stateLock = new object();
            _controlLock = new object();

            _state = WorkerState.Stopped;

            _stateSignals = Enum
                .GetValues(typeof(WorkerState))
                .Cast<WorkerState>()
                .ToDictionary(x => x, x => new AutoResetEvent(false));
        }

        #endregion

        #region Abstract

        protected abstract void StartImpl();
        protected abstract void PauseImpl();
        protected abstract void ResumeImpl();
        protected abstract void StopImpl();
        protected abstract void DisposeImpl();

        #endregion

        #region Protected

        protected void LogDebug(string message, int shiftFromCaller = 0)
        {
            StackTrace stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(1 + shiftFromCaller);
            var method = frame.GetMethod();

            var debugMessage = $"[{this.Name}][{this.GetType().Name}.{method.Name}] {message}";
            Log.Debug(debugMessage);

            Log.ForContext("taucode.working", true).Debug(debugMessage);
        }

        protected void LogError(string message, int shiftFromCaller = 0)
        {
            StackTrace stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(1 + shiftFromCaller);
            var method = frame.GetMethod();

            var information = $"[{this.Name}][{method.Name}] {message}";
            Log.Error(information);
        }

        protected void ChangeState(WorkerState state)
        {
            lock (_stateLock)
            {
                _state = state;

                this.LogDebug($"State changed to '{_state}'");

                _stateSignals[_state].Set();
            }
        }

        protected WorkingException CreateInternalErrorException() => new WorkingException("Internal error.");

        protected void CheckInternalIntegrity(bool condition)
        {
            if (!condition)
            {
                throw this.CreateInternalErrorException();
            }
        }

        #endregion

        #region Private

        protected void CheckStateForOperation(params WorkerState[] acceptedStates)
        {
            var state = this.State;

            if (!acceptedStates.Contains(state))
            {
                var sb = new StringBuilder();
                sb.Append($"To perform this operation, '{nameof(State)}' must be ");

                for (var i = 0; i < acceptedStates.Length; i++)
                {
                    var acceptedState = acceptedStates[i];
                    sb.Append($"'{acceptedState}'");

                    if (i < acceptedStates.Length - 2)
                    {
                        sb.Append(", ");
                    }
                    else if (i < acceptedStates.Length - 1)
                    {
                        sb.Append(" or ");
                    }
                }

                sb.Append($" while actually it is '{state}'.");

                throw new WorkingException(sb.ToString());
            }
        }

        // todo: CheckStateForOperation and CheckState are almost copy/paste.
        protected void CheckState(params WorkerState[] acceptedStates)
        {
            var state = this.State;

            if (!acceptedStates.Contains(state))
            {
                var sb = new StringBuilder();
                sb.Append($"'{nameof(State)}' is expected to be ");

                for (var i = 0; i < acceptedStates.Length; i++)
                {
                    var acceptedState = acceptedStates[i];
                    sb.Append($"'{acceptedState}'");

                    if (i < acceptedStates.Length - 2)
                    {
                        sb.Append(", ");
                    }
                    else if (i < acceptedStates.Length - 1)
                    {
                        sb.Append(" or ");
                    }
                }

                sb.Append($" while actually it is '{state}'.");

                throw new WorkingException(sb.ToString());
            }
        }

        #endregion

        #region IWorker Members

        public string Name
        {
            get
            {
                lock (_stateLock)
                {
                    return _name;
                }
            }
            set
            {
                lock (_stateLock)
                {
                    _name = value;
                }
            }
        }

        public WorkerState State
        {
            get
            {
                lock (_stateLock)
                {
                    return _state;
                }
            }
        }

        public void Start()
        {
            this.LogDebug("Start requested");

            lock (_controlLock)
            {
                this.CheckStateForOperation(WorkerState.Stopped);
                this.StartImpl();
                this.CheckState(WorkerState.Running);
            }
        }

        public void Pause()
        {
            this.LogDebug("Pause requested");

            lock (_controlLock)
            {
                this.CheckStateForOperation(WorkerState.Running);
                this.PauseImpl();
                this.CheckState(WorkerState.Paused);
            }
        }

        public void Resume()
        {
            this.LogDebug("Resume requested");

            lock (_controlLock)
            {
                this.CheckStateForOperation(WorkerState.Paused);
                this.ResumeImpl();
                this.CheckState(WorkerState.Running);
            }
        }

        public void Stop()
        {
            this.LogDebug("Stop requested");

            lock (_controlLock)
            {
                this.CheckStateForOperation(WorkerState.Running, WorkerState.Paused);
                this.StopImpl();
                this.CheckState(WorkerState.Stopped);
            }
        }

        public WorkerState? WaitForStateChange(int millisecondsTimeout, params WorkerState[] states)
        {
            if (states.Length == 0)
            {
                throw new ArgumentException($"'{nameof(states)}' cannot be empty.");
            }

            var state = this.State;
            if (state == WorkerState.Disposed || state == WorkerState.Disposing)
            {
                var objectName = $"{this.GetType()} Name: {this.Name ?? "null"}";

                throw new ObjectDisposedException(objectName,
                    $"Cannot wait for state change of a worker which has state '{state}'.");
            }

            // Between previous check and following code 'State' might be changed to 'Disposed' or 'Disposing'.
            // Therefore, handles might be disposed.
            // But that's not our problem anymore. We've tried to warn!


            var distinctStates = states
                .Distinct()
                .ToArray();

            var handles = distinctStates
                .Select(x => _stateSignals[x])
                .Cast<WaitHandle>()
                .ToArray();

            var tuples = Enumerable
                .Range(0, distinctStates.Length)
                .ToDictionary(x => x, x => Tuple.Create(x, distinctStates[x], _stateSignals[distinctStates[x]]));

            var handleIndex = WaitHandle.WaitAny(handles, millisecondsTimeout);

            if (handleIndex == WaitHandle.WaitTimeout)
            {
                // timeout.
                return null;
            }

            var gotState = tuples[handleIndex].Item2;
            return gotState;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            this.LogDebug("Dispose requested");

            lock (_controlLock)
            {
                this.CheckStateForOperation(WorkerState.Stopped, WorkerState.Running, WorkerState.Paused);
                this.DisposeImpl();
                this.CheckState(WorkerState.Disposed);

                foreach (var signal in _stateSignals.Values)
                {
                    signal.Dispose();
                }

                _stateSignals.Clear();
            }
        }

        #endregion
    }
}
