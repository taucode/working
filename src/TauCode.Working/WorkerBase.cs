using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Text;
using System.Threading;
using TauCode.Working.Exceptions;

namespace TauCode.Working
{
    public abstract class WorkerBase : IWorker
    {
        #region Fields

        private long _stateValue;
        private long _isDisposedValue;

        private string _name;
        private ILogger _logger;

        private readonly object _dataLock;

        private readonly object _controlLock;

        #endregion

        #region Constructor

        protected WorkerBase()
        {
            _dataLock = new object();
            _controlLock = new object();

            this.SetState(WorkerState.Stopped);
            this.SetIsDisposed(false);
        }

        #endregion

        #region Private

        private WorkerState GetState()
        {
            var stateValue = Interlocked.Read(ref _stateValue);
            return (WorkerState)stateValue;
        }

        private void SetState(WorkerState state)
        {
            var stateValue = (long)state;
            Interlocked.Exchange(ref _stateValue, stateValue);
        }

        private bool GetIsDisposed()
        {
            var isDisposedValue = Interlocked.Read(ref _isDisposedValue);
            return isDisposedValue == 1L;
        }

        private void SetIsDisposed(bool isDisposed)
        {
            var isDisposedValue = isDisposed ? 1L : 0L;
            Interlocked.Exchange(ref _isDisposedValue, isDisposedValue);
        }

        private NotSupportedException CreatePausingNotSupportedException()
        {
            return new NotSupportedException("Pausing/resuming is not supported.");
        }

        #endregion

        #region Abstract

        public abstract bool IsPausingSupported { get; }

        protected abstract void OnStarting();
        protected abstract void OnStarted();

        protected abstract void OnStopping();
        protected abstract void OnStopped();

        protected abstract void OnPausing();
        protected abstract void OnPaused();

        protected abstract void OnResuming();
        protected abstract void OnResumed();

        protected abstract void OnDisposed();

        #endregion

        #region Protected

        protected ILogger GetSafeLogger() => this.Logger ?? NullLogger.Instance;

        protected void Stop(bool throwOnDisposedOrWrongState)
        {
            lock (_controlLock)
            {
                if (this.GetIsDisposed())
                {
                    if (throwOnDisposedOrWrongState)
                    {
                        throw this.CreateObjectDisposedException(nameof(Stop));
                    }
                    else
                    {
                        return;
                    }
                }

                var state = this.GetState();
                var isValidState =
                    state == WorkerState.Running ||
                    state == WorkerState.Paused;

                if (!isValidState)
                {
                    if (throwOnDisposedOrWrongState)
                    {
                        throw this.CreateInvalidWorkerOperationException(nameof(Stop), state);
                    }
                    else
                    {
                        return;
                    }
                }

                this.SetState(WorkerState.Stopping);
                this.GetSafeLogger().LogDebug($"Worker '{this.Name}' is stopping.");
                this.OnStopping();

                this.SetState(WorkerState.Stopped);
                this.GetSafeLogger().LogDebug($"Worker '{this.Name}' is stopped.");
                this.OnStopped();
            }
        }

        protected ObjectDisposedException CreateObjectDisposedException(string requestedOperation)
        {
            var sb = new StringBuilder();
            sb.Append($"Cannot perform operation '{requestedOperation}' because worker is disposed.");

            var message = sb.ToString();
            return new ObjectDisposedException(this.Name, message);
        }

        protected InvalidWorkerOperationException CreateInvalidWorkerOperationException(string requestedOperation, WorkerState state)
        {
            var sb = new StringBuilder();
            sb.Append($"Cannot perform operation '{requestedOperation}'. Worker state is '{state}'.");
            if (this.Name != null)
            {
                sb.Append($" Worker name is '{this.Name}'.");
            }

            var message = sb.ToString();

            return new InvalidWorkerOperationException(message, this.Name);
        }

        #endregion

        #region IWorker Members

        public string Name
        {
            get
            {
                lock (_dataLock)
                {
                    return _name;
                }
            }
            set
            {
                if (this.GetIsDisposed())
                {
                    throw this.CreateObjectDisposedException($"set {nameof(Name)}");
                }

                lock (_dataLock)
                {
                    _name = value;
                }
            }
        }

        public WorkerState State => this.GetState();

        public void Start()
        {
            lock (_controlLock)
            {
                if (this.GetIsDisposed())
                {
                    throw this.CreateObjectDisposedException(nameof(Start));
                }

                var state = this.GetState();
                if (state != WorkerState.Stopped)
                {
                    throw this.CreateInvalidWorkerOperationException(nameof(Start), state);

                }

                this.SetState(WorkerState.Starting);
                this.GetSafeLogger().LogDebug($"Worker '{this.Name}' is starting.");
                this.OnStarting();

                this.SetState(WorkerState.Running);
                this.GetSafeLogger().LogDebug($"Worker '{this.Name}' is started.");
                this.OnStarted();
            }
        }

        public void Stop() => this.Stop(true);

        public void Pause()
        {
            if (!IsPausingSupported)
            {
                throw this.CreatePausingNotSupportedException();
            }

            lock (_controlLock)
            {
                if (this.GetIsDisposed())
                {
                    throw this.CreateObjectDisposedException(nameof(Pause));
                }

                var state = this.GetState();
                if (state != WorkerState.Running)
                {
                    throw this.CreateInvalidWorkerOperationException(nameof(Pause), state);
                }

                this.SetState(WorkerState.Pausing);
                this.GetSafeLogger().LogDebug($"Worker '{this.Name}' is pausing.");
                this.OnPausing();

                this.SetState(WorkerState.Paused);
                this.GetSafeLogger().LogDebug($"Worker '{this.Name}' is paused.");
                this.OnPaused();
            }
        }

        public void Resume()
        {
            if (!IsPausingSupported)
            {
                throw this.CreatePausingNotSupportedException();
            }

            lock (_controlLock)
            {
                if (this.GetIsDisposed())
                {
                    throw this.CreateObjectDisposedException(nameof(Resume));
                }

                var state = this.GetState();
                if (state != WorkerState.Paused)
                {
                    throw this.CreateInvalidWorkerOperationException(nameof(Resume), state);
                }

                this.SetState(WorkerState.Resuming);
                this.GetSafeLogger().LogDebug($"Worker '{this.Name}' is resuming.");
                this.OnResuming(); // todo: try/catch, here & anywhere? implementation of abstract method might throw...

                this.SetState(WorkerState.Running);
                this.GetSafeLogger().LogDebug($"Worker '{this.Name}' is resumed.");
                this.OnResumed();
            }
        }

        public bool IsDisposed => this.GetIsDisposed();
        public ILogger Logger
        {
            get
            {
                lock (_dataLock)
                {
                    return _logger;
                }
            }
            set
            {
                if (this.GetIsDisposed())
                {
                    throw this.CreateObjectDisposedException($"set {nameof(Logger)}");
                }

                lock (_dataLock)
                {
                    _logger = value;
                }
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            lock (_controlLock)
            {
                if (this.GetIsDisposed())
                {
                    return; // won't dispose twice
                }

                this.Stop(false);

                this.SetIsDisposed(true);

                this.OnDisposed();
            }
        }

        #endregion
    }
}
