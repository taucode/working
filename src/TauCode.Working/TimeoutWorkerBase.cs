using System;
using System.Threading;
using System.Threading.Tasks;

namespace TauCode.Working
{
    public abstract class TimeoutWorkerBase : LoopWorkerBase, ITimeoutWorker
    {
        #region Constants

        private const int ChangeTimeoutSignalIndex = 1;

        #endregion


        #region Fields

        private readonly object _timeoutLock;
        private TimeSpan _timeout;
        private readonly AutoResetEvent _changeTimeoutSignal;

        #endregion

        #region Constructors

        protected TimeoutWorkerBase(TimeSpan initialTimeout)
        {
            this.CheckTimeoutArgument(initialTimeout);
            _timeout = initialTimeout;

            _timeoutLock = new object();
            _changeTimeoutSignal = new AutoResetEvent(false);
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

        protected override AutoResetEvent[] GetExtraSignals()
        {
            return new[] { _changeTimeoutSignal };
        }

        protected override async Task<WorkFinishReason> DoWorkAsyncImpl()
        {
            await this.DoRealWorkAsync();
            return WorkFinishReason.WorkIsDone;
        }

        protected override Task<VacationFinishReason> TakeVacationAsyncImpl()
        {
            var index = this.WaitForControlSignalWithExtraSignals(this.Timeout); // todo

            switch (index)
            {
                case ControlSignalIndex:
                    return Task.FromResult(VacationFinishReason.GotControlSignal);

                case ChangeTimeoutSignalIndex:
                    throw new NotImplementedException();
                    //return VacationFinishedReason.NewWorkArrived;

                case WaitHandle.WaitTimeout:
                    return Task.FromResult(VacationFinishReason.VacationTimeElapsed);

                default:
                    throw this.CreateInternalErrorException();
            }
        }

        #endregion

        #region Private

        private void CheckTimeoutArgument(in TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero)
            {
                throw new ArgumentException($"'{timeout}' must be positive.");
            }
        }

        #endregion

        #region ITimeoutWorker Members

        public TimeSpan Timeout
        {
            get
            {
                lock (_timeoutLock)
                {
                    return _timeout;
                }
            }
            set
            {
                this.CheckTimeoutArgument(value);
                lock (_timeoutLock)
                {
                    _timeout = value;
                }
            }
        }

        #endregion
    }
}
