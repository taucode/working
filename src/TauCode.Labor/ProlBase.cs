using System;
using System.Threading;
using TauCode.Labor.Exceptions;

namespace TauCode.Labor
{
    public class ProlBase : IProl
    {
        #region Fields

        private long _stateValue;
        private long _isDisposedValue;

        protected readonly object Lock;


        #endregion

        #region Constructor

        public ProlBase()
        {
            this.Lock = new object();
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

        protected virtual void OnStarting()
        {
            // idle
        }

        protected virtual void OnStarted()
        {
            // idle
        }

        protected virtual void OnStopping()
        {
            // idle
        }

        protected virtual void OnStopped()
        {
            // idle
        }

        #endregion

        #region IProl Members

        public ProlState State => this.GetState();

        public void Start()
        {
            lock (this.Lock)
            {
                if (this.GetIsDisposed())
                {
                    throw new NotImplementedException(); // cannot start
                }

                if (this.GetState() != ProlState.Stopped)
                {
                    throw new InappropriateProlStateException();
                }

                this.SetState(ProlState.Starting);
                this.OnStarting();

                this.SetState(ProlState.Running);
                this.OnStarted();
            }
        }

        public void Stop()
        {
            lock (this.Lock)
            {
                if (this.GetIsDisposed())
                {
                    throw new NotImplementedException(); // cannot stop
                }

                if (this.GetState() != ProlState.Running)
                {
                    throw new InappropriateProlStateException(); // todo
                }

                this.SetState(ProlState.Stopping);
                this.OnStopping();

                this.SetState(ProlState.Stopped);
                this.OnStopped();
            }
        }

        public bool IsDisposed => this.GetIsDisposed();

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            lock (this.Lock)
            {
                if (this.GetIsDisposed())
                {
                    return; // won't dispose twice
                }

                var state = this.GetState();

                if (state == ProlState.Running)
                {
                    this.Stop();
                }

                this.SetIsDisposed(true);
            }
        }

        #endregion
    }
}
