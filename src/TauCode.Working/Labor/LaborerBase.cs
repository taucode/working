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

        #region Protected

        protected void Start(bool throwOnDisposedOrWrongState)
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
                if (state != LaborerState.Stopped)
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

                this.SetState(LaborerState.Starting);
                this.OnStarting();

                this.SetState(LaborerState.Running);
                this.OnStarted();
            }
        }

        protected virtual void OnStarting()
        {
            // idle
        }

        protected virtual void OnStarted()
        {
            // idle
        }

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

        protected virtual void OnStopping()
        {
            // idle
        }

        protected virtual void OnStopped()
        {
            // idle
        }

        protected virtual void OnDisposed()
        {
            // idle
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
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }

        public bool IsDisposed => this.GetIsDisposed();

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
