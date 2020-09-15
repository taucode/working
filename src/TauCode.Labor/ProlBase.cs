﻿using System;
using System.Threading;
using TauCode.Labor.Exceptions;

namespace TauCode.Labor
{
    public class ProlBase : IProl
    {
        #region Fields

        private long _stateValue;
        private long _isDisposedValue;

        private string _name;
        private readonly object _nameLock;

        private readonly object _lock;

        #endregion

        #region Constructor

        public ProlBase()
        {
            _lock = new object();
            _nameLock = new object();

            this.SetState(ProlState.Stopped);
            this.SetIsDisposed(false);
        }

        #endregion

        #region Private

        private ProlState GetState()
        {
            var stateValue = Interlocked.Read(ref _stateValue);
            return (ProlState)stateValue;
        }

        private void SetState(ProlState state)
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
            lock (_lock)
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

                if (this.GetState() != ProlState.Stopped)
                {
                    if (throwOnDisposedOrWrongState)
                    {
                        throw new InappropriateProlStateException();
                    }
                    else
                    {
                        return;
                    }
                }

                this.SetState(ProlState.Starting);
                this.OnStarting();

                this.SetState(ProlState.Running);
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
            lock (_lock)
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

                if (this.GetState() != ProlState.Running)
                {
                    if (throwOnDisposedOrWrongState)
                    {
                        throw new InappropriateProlStateException();
                    }
                    else
                    {
                        return;
                    }
                }

                this.SetState(ProlState.Stopping);
                this.OnStopping();

                this.SetState(ProlState.Stopped);
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

        #region IProl Members

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
                lock (_nameLock)
                {
                    _name = value;
                }
            }
        }

        public ProlState State => this.GetState();

        public void Start() => this.Start(true);

        public void Stop() => this.Stop(true);

        public bool IsDisposed => this.GetIsDisposed();

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            lock (_lock)
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
