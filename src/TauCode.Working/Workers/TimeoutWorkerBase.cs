using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// todo clean up
namespace TauCode.Working.Workers
{
    public abstract class TimeoutWorkerBase : LoopWorkerBase, ITimeoutWorker
    {
        #region Constants

        private const int ChangeTimeoutSignalIndex = 1;

        #endregion

        #region Fields

        private TimeSpan _timeout;
        private AutoResetEvent _changeTimeoutSignal; // disposed by LoopWorkerBase.Shutdown

        #endregion

        #region Constructors

        protected TimeoutWorkerBase(TimeSpan initialTimeout)
        {
            this.CheckTimeoutArgument(initialTimeout);
            _timeout = initialTimeout;
        }

        protected TimeoutWorkerBase(int initialMillisecondsTimeout)
            : this(TimeSpan.FromMilliseconds(initialMillisecondsTimeout))
        {
        }

        #endregion

        #region Abstract

        protected abstract Task DoRealWorkAsync();

        #endregion

        #region Overridden

        protected override IList<AutoResetEvent> CreateExtraSignals()
        {
            _changeTimeoutSignal = new AutoResetEvent(false);
            return new[] { /*_changeTimeoutSignal*/ _changeTimeoutSignal };
        }

        protected override async Task<WorkFinishReason> DoWorkAsyncImpl()
        {
            await this.DoRealWorkAsync();
            return WorkFinishReason.WorkIsDone;
        }

        protected override Task<VacationFinishReason> TakeVacationAsyncImpl()
        {
            var index = this.WaitForControlSignalWithExtraSignals(this.Timeout);

            switch (index)
            {
                case ControlSignalIndex:
                    return Task.FromResult(VacationFinishReason.GotControlSignal);

                case ChangeTimeoutSignalIndex:
                    return Task.FromResult(VacationFinishReason.NewWorkArrived);

                case WaitHandle.WaitTimeout:
                    return Task.FromResult(VacationFinishReason.VacationTimeElapsed);

                default:
                    throw this.CreateInternalErrorException();
            }
        }

        protected override void Shutdown(WorkerState shutdownState)
        {
            base.Shutdown(shutdownState);
            _changeTimeoutSignal = null;
        }

        #endregion

        #region Private

        private void CheckTimeoutArgument(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero)
            {
                throw new ArgumentException($"'{nameof(timeout)}' must be positive.");
            }
        }

        #endregion

        #region ITimeoutWorker Members

        public TimeSpan Timeout
        {
            get
            {
                TimeSpan? result = null;

                this.InvokeWithControlLock(() =>
                {
                    this.CheckState("Timeout value 'get' is requested.", WorkingExtensions.NonDisposedStates);
                    result = _timeout;
                });

                return result ?? throw this.CreateInternalErrorException();
            }
            set
            {
                this.CheckTimeoutArgument(value);

                this.InvokeWithControlLock(() =>
                {
                    this.CheckState("Timeout value 'set' is requested.", WorkingExtensions.NonDisposedStates);
                    _timeout = value;
                    _changeTimeoutSignal?.Set();
                });
            }
        }

        #endregion
    }
}
