using Microsoft.Extensions.Logging;
using System;
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

        protected void Stop(bool throwOnDisposedOrWrongState)
        {
            lock (_controlLock)
            {
                if (this.GetIsDisposed())
                {
                    if (throwOnDisposedOrWrongState)
                    {
                        throw new ObjectDisposedException(this.Name);
                    }
                    else
                    {
                        return;
                    }
                }

                var state = this.GetState();
                if (state != LaborerState.Running)
                {
                    if (throwOnDisposedOrWrongState)
                    {
                        throw new InappropriateLaborerStateException(state);
                    }
                    else
                    {
                        return;
                    }
                }

                this.SetState(LaborerState.Stopping);
                this.OnStopping();

                this.SetState(LaborerState.Stopped);
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
                    throw new ObjectDisposedException(this.Name);
                }

                var state = this.GetState();
                if (state != LaborerState.Stopped)
                {
                    throw new InappropriateLaborerStateException(state);
                }

                this.SetState(LaborerState.Starting);
                this.OnStarting();

                this.SetState(LaborerState.Running);
                this.OnStarted();
            }
        }

        public void Stop() => this.Stop(true);

        public void Pause()
        {
            if (!IsPausingSupported)
            {
                throw new NotSupportedException("Pausing/resuming is not supported."); // todo: copy/pasted
            }

            lock (_controlLock)
            {
                if (this.GetIsDisposed())
                {
                    throw new ObjectDisposedException(this.Name);
                }

                var state = this.GetState();
                if (state != LaborerState.Running)
                {
                    throw new InappropriateLaborerStateException(state);
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
                throw new NotSupportedException("Pausing/resuming is not supported.");
            }

            lock (_controlLock)
            {
                if (this.GetIsDisposed())
                {
                    throw new ObjectDisposedException(this.Name);
                }

                var state = this.GetState();
                if (state != LaborerState.Paused)
                {
                    throw new InappropriateLaborerStateException(state);
                }

                this.SetState(LaborerState.Resuming);
                this.OnResuming();

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
