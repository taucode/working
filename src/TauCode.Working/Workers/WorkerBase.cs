using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TauCode.Working.Exceptions;
using TauCode.Working.Logging;

namespace TauCode.Working.Workers
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

        private WorkerLogger _logger;

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

        protected virtual WorkerLogger CreateLogger()
        {
            return new WorkerLogger(this);
        }

        protected WorkerLogger GetLogger() => _logger ??= this.CreateLogger();

        //protected void LogDebug(string methodName, string message)
        //{
        //    //StackTrace stackTrace = new StackTrace();
        //    //var frame = stackTrace.GetFrame(1 + shiftFromCaller);
        //    //var method = frame.GetMethod();

        //    //var debugMessage = $"[{this.Name}][{this.GetType().Name}.{method.Name}] {message}";
        //    //Log.Debug(debugMessage);

        //    //Log.ForContext("taucode.working", true).Debug(debugMessage);
        //}

        //protected void LogError(string methodName, string message, int shiftFromCaller = 0)
        //{
        //    throw new NotImplementedException();

        //    //StackTrace stackTrace = new StackTrace();
        //    //var frame = stackTrace.GetFrame(1 + shiftFromCaller);
        //    //var method = frame.GetMethod();

        //    //var information = $"[{this.Name}][{method.Name}] {message}";
        //    //Log.Error(information);
        //}

        protected void ChangeState(WorkerState state)
        {
            lock (_stateLock)
            {
                _state = state;

                //this.LogDebug($"State changed to '{_state}'.");
                this.GetLogger().Debug($"State changed to '{_state}'.", nameof(ChangeState));

                _stateSignals[_state].Set();
            }
        }

        protected void InvokeWithControlLock(Action action)
        {
            lock (_controlLock)
            {
                action();
            }
        }

        protected T GetWithControlLock<T>(Func<T> getter)
        {
            lock (_controlLock)
            {
                var value = getter();
                return value;
            }
        }

        protected WorkingException CreateInternalErrorException() => new WorkingException("Internal error.");

        // todo: consider HashSet.
        protected void CheckState(string preamble, params WorkerState[] acceptedStates)
        {
            var state = this.State;
            if (acceptedStates.Contains(state))
            {
                // ok.
            }
            else
            {
                var sb = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(preamble))
                {
                    sb.AppendLine(preamble);
                }

                var acceptedStatesString = string.Join(", ", acceptedStates);
                sb.AppendLine($"'{nameof(this.State)}' is expected to be one of the following: [{acceptedStatesString}].");
                sb.Append($"Actual value: {state}.");

                throw new ForbiddenWorkerStateException(
                    this.Name,
                    acceptedStates,
                    state,
                    sb.ToString());
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
            lock (_controlLock)
            {
                var message = $"'{nameof(Start)}' requested.";

                //this.LogDebug(message);
                this.GetLogger().Debug(message, nameof(Start));

                this.CheckState(message, WorkerState.Stopped);

                this.StartImpl();

                message = $"'{nameof(StartImpl)}' executed.";

                //this.LogDebug(message);
                this.GetLogger().Debug(message, nameof(Start));

                this.CheckState(message, WorkerState.Running);
            }
        }

        public void Pause()
        {

            lock (_controlLock)
            {
                var message = $"'{nameof(Pause)}' requested.";

                //this.LogDebug(message);
                this.GetLogger().Debug(message, nameof(Pause));

                this.CheckState(message, WorkerState.Running);

                this.PauseImpl();

                message = $"'{nameof(PauseImpl)}' executed.";

                //this.LogDebug(message);
                this.GetLogger().Debug(message, nameof(Pause));

                this.CheckState(message, WorkerState.Paused);
            }
        }

        public void Resume()
        {
            lock (_controlLock)
            {
                var message = $"'{nameof(Resume)}' requested.";

                //this.LogDebug(message);
                this.GetLogger().Debug(message, nameof(Resume));

                this.CheckState(message, WorkerState.Paused);

                this.ResumeImpl();

                message = $"'{nameof(ResumeImpl)}' executed.";

                //this.LogDebug(message);
                this.GetLogger().Debug(message, nameof(Resume));

                this.CheckState(message, WorkerState.Running);
            }
        }

        public void Stop()
        {
            lock (_controlLock)
            {
                var message = $"'{nameof(Stop)}' requested.";

                //this.LogDebug(message);
                this.GetLogger().Debug(message, nameof(Stop));

                this.CheckState(message, WorkerState.Running, WorkerState.Paused);

                this.StopImpl();

                message = $"'{nameof(StopImpl)}' executed.";

                //this.LogDebug(message);
                this.GetLogger().Debug(message, nameof(Stop));

                this.CheckState(message, WorkerState.Stopped);
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
            lock (_controlLock)
            {
                if (_state == WorkerState.Disposed)
                {
                    return; // already disposed
                }

                var message = $"'{nameof(Dispose)}' requested.";

                //this.LogDebug(message);
                this.GetLogger().Debug(message, nameof(Dispose));

                this.CheckState(message, WorkerState.Stopped, WorkerState.Running, WorkerState.Paused);
                
                this.DisposeImpl();

                message = $"'{nameof(DisposeImpl)}' executed.";

                //this.LogDebug(message);
                this.GetLogger().Debug(message, nameof(Dispose));

                this.CheckState(message, WorkerState.Disposed);

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
