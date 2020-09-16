using System;
using System.Threading;
using TauCode.Working.Exceptions;

namespace TauCode.Working
{
    public class WorkerBase : IWorker
    {
        #region Fields

        private long _stateValue;
        private long _isDisposedValue;

        private string _name;
        private readonly object _nameLock;

        private readonly object _lock;

        #endregion

        #region Constructor

        public WorkerBase()
        {
            _lock = new object();
            _nameLock = new object();

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

                if (this.GetState() != WorkerState.Stopped)
                {
                    if (throwOnDisposedOrWrongState)
                    {
                        throw new InappropriateWorkerStateException();
                    }
                    else
                    {
                        return;
                    }
                }

                this.SetState(WorkerState.Starting);
                this.OnStarting();

                this.SetState(WorkerState.Running);
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

                if (this.GetState() != WorkerState.Running)
                {
                    if (throwOnDisposedOrWrongState)
                    {
                        throw new InappropriateWorkerStateException();
                    }
                    else
                    {
                        return;
                    }
                }

                this.SetState(WorkerState.Stopping);
                this.OnStopping();

                this.SetState(WorkerState.Stopped);
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

        #region IWorker Members

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

        public WorkerState State => this.GetState();

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
