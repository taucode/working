using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Text;
using System.Threading;
using TauCode.Working.Labor.Exceptions;

namespace TauCode.Working.Labor
{
    public abstract class LaborerBase : ILaborer
    {
        #region Fields

        private long _stateValue;
        private long _isDisposedValue;

        private string _name;
        private readonly object _nameLock;

        private readonly object _controlLock;

        #endregion

        #region Constructor

        protected LaborerBase()
        {
            _nameLock = new object();
            _controlLock = new object();

            this.SetState(LaborerState.Stopped);
            this.SetIsDisposed(false);
        }

        #endregion

        #region Private

        private LaborerState GetState()
        {
            var stateValue = Interlocked.Read(ref _stateValue);
            return (LaborerState)stateValue;
        }

        private void SetState(LaborerState state)
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

        private InvalidLaborerOperationException CreateInvalidLaborerOperationException(string requestedOperation, LaborerState state)
        {
            var sb = new StringBuilder();
            sb.Append($"Cannot perform operation '{requestedOperation}'. Laborer state is '{state}'.");
            if (this.Name != null)
            {
                sb.Append($" Laborer name is '{this.Name}'.");
            }

            var message = sb.ToString();

            return new InvalidLaborerOperationException(message, this.Name);
        }

        private ObjectDisposedException CreateObjectDisposedException(string requestedOperation)
        {
            var sb = new StringBuilder();
            sb.Append($"Cannot perform operation '{requestedOperation}' because laborer is disposed.");

            var message = sb.ToString();
            return new ObjectDisposedException(this.Name, message);
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
                    state == LaborerState.Running ||
                    state == LaborerState.Paused;

                if (!isValidState)
                {
                    if (throwOnDisposedOrWrongState)
                    {
                        throw this.CreateInvalidLaborerOperationException(nameof(Stop), state);
                    }
                    else
                    {
                        return;
                    }
                }

                this.SetState(LaborerState.Stopping);
                this.GetSafeLogger().LogDebug($"Laborer '{this.Name}' is stopping.");
                this.OnStopping();

                this.SetState(LaborerState.Stopped);
                this.GetSafeLogger().LogDebug($"Laborer '{this.Name}' is stopped.");
                this.OnStopped();
            }
        }

        #endregion

        #region ILaborer Members

        public string Name
        {
            get
            {
                lock (_nameLock)
                {
                    return _name;
                }
            }
            set
            {
                if (this.GetIsDisposed())
                {
                    throw new ObjectDisposedException(this.Name);
                }

                lock (_nameLock)
                {
                    _name = value;
                }
            }
        }

        public LaborerState State => this.GetState();

        public void Start()
        {
            lock (_controlLock)
            {
                if (this.GetIsDisposed())
                {
                    throw this.CreateObjectDisposedException(nameof(Start));
                }

                var state = this.GetState();
                if (state != LaborerState.Stopped)
                {
                    throw this.CreateInvalidLaborerOperationException(nameof(Start), state);

                }

                this.SetState(LaborerState.Starting);
                this.GetSafeLogger().LogDebug($"Laborer '{this.Name}' is starting.");
                this.OnStarting();

                this.SetState(LaborerState.Running);
                this.GetSafeLogger().LogDebug($"Laborer '{this.Name}' is started.");
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
                if (state != LaborerState.Running)
                {
                    throw this.CreateInvalidLaborerOperationException(nameof(Pause), state);
                }

                this.SetState(LaborerState.Pausing);
                this.OnPausing();

                this.SetState(LaborerState.Paused);
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
                if (state != LaborerState.Paused)
                {
                    throw this.CreateInvalidLaborerOperationException(nameof(Resume), state);
                }

                this.SetState(LaborerState.Resuming);
                this.OnResuming(); // todo: try/catch, here & anywhere? implementation of abstract method might throw...

                this.SetState(LaborerState.Running);
                this.OnResumed();
            }
        }

        public bool IsDisposed => this.GetIsDisposed();
        public ILogger Logger { get; set; } // todo thread safe

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
