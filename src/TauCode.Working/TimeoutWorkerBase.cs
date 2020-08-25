using System;
using System.Threading;
using System.Threading.Tasks;

namespace TauCode.Working
{
    public abstract class TimeoutWorkerBase : LoopWorkerBase, ITimeoutWorker
    {
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

        protected TimeoutWorkerBase(int initialTimeoutMilliseconds)
            : this(TimeSpan.FromMilliseconds(initialTimeoutMilliseconds))
        {
        }

        #endregion

        #region Abstract

        protected abstract Task DoRealWorkAsync();

        #endregion

        #region Overridden

        protected override async Task<WorkFinishReason> DoWorkAsync()
        {
            await this.DoRealWorkAsync();
            return WorkFinishReason.WorkIsDone;
        }

        protected override VacationFinishedReason TakeVacation()
        {
            this.WaitForControlSignalWithExtraSignals(11); // todo
            throw new NotImplementedException();
        }

        #endregion

        #region Private

        private void CheckTimeoutArgument(in TimeSpan timeout)
        {
            throw new NotImplementedException();
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
