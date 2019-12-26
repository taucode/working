using Serilog;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TauCode.Working.Lab
{
    public abstract class WorkerBase : IWorker
    {
        #region Fields

        private string _name;
        private WorkerState _state;
        private readonly object _stateLock;
        private readonly object _controlLock;

        #endregion

        #region Constructor

        protected WorkerBase()
        {
            _stateLock = new object();
            _controlLock = new object();

            _state = WorkerState.Stopped;
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

        protected void LogVerbose(string message, int shiftFromCaller = 0)
        {
            StackTrace stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(1 + shiftFromCaller);
            var method = frame.GetMethod();

            var information = $"[{this.Name}][{method.Name}] {message}";
            Log.Verbose(information);
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
            this.LogVerbose("Start requested");

            lock (_controlLock)
            {
                this.CheckStateForOperation(WorkerState.Stopped);
                this.StartImpl();
                this.CheckState(WorkerState.Running);
            }
        }

        public void Pause()
        {
            lock (_controlLock)
            {
                this.CheckStateForOperation(WorkerState.Running);
                this.PauseImpl();
                this.CheckState(WorkerState.Paused);
            }
        }

        public void Resume()
        {
            lock (_controlLock)
            {
                this.CheckStateForOperation(WorkerState.Paused);
                this.ResumeImpl();
                this.CheckState(WorkerState.Running);
            }
        }

        public void Stop()
        {
            lock (_controlLock)
            {
                this.CheckStateForOperation(WorkerState.Running, WorkerState.Paused);
                this.StopImpl();
                this.CheckState(WorkerState.Stopped);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            lock (_controlLock)
            {
                this.CheckStateForOperation(WorkerState.Stopped, WorkerState.Running, WorkerState.Paused);
                this.DisposeImpl();
                this.CheckState(WorkerState.Disposed);
            }
        }

        #endregion
    }
}
