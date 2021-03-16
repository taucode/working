using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Extensions;

namespace TauCode.Working.Labor
{
    public abstract class LoopLaborerBase : LaborerBase
    {
        #region Constants

        protected readonly TimeSpan VeryLongVacation = TimeSpan.FromMilliseconds(int.MaxValue);
        protected readonly TimeSpan TimeQuantum = TimeSpan.FromMilliseconds(1);
        protected readonly TimeSpan DefaultErrorTimeout = TimeSpan.FromSeconds(5);

        #endregion

        #region Fields

        private TimeSpan _errorTimeout;
        private readonly object _errorTimeoutLock;

        private readonly object _startingLock;
        private readonly object _runningLock;

        private Task _loopTask;

        private CancellationTokenSource _signal;
        
        #endregion

        #region Constructor

        protected LoopLaborerBase()
        {
            _startingLock = new object();
            _runningLock = new object();

            _errorTimeout = DefaultErrorTimeout;
            _errorTimeoutLock = new object();
        }

        #endregion

        #region Abstract

        protected abstract Task<TimeSpan> DoLabor(CancellationToken cancellationToken);

        #endregion

        #region Overridden

        protected override void OnStarting()
        {
            _loopTask = this.LoopRoutine();

            lock (_startingLock)
            {
                _loopTask.Start();
                Monitor.Wait(_startingLock);
            }
        }

        protected override void OnStarted()
        {
            lock (_runningLock)
            {
                _signal = new CancellationTokenSource();
                Monitor.Pulse(_runningLock);
            }
        }

        protected override void OnStopping()
        {
            throw new NotImplementedException();
        }

        protected override void OnStopped()
        {
            throw new NotImplementedException();
        }

        protected override void OnPausing()
        {
            throw new NotImplementedException();
        }

        protected override void OnPaused()
        {
            throw new NotImplementedException();
        }

        protected override void OnResuming()
        {
            throw new NotImplementedException();
        }

        protected override void OnResumed()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private

        private async Task SleepAfterError()
        {
            try
            {
                await Task.Delay(this.ErrorTimeout, _signal.Token);
            }
            catch (TaskCanceledException)
            {
                // dismiss.
            }
        }

        private async Task LoopRoutine()
        {
            lock (_runningLock)
            {
                lock (_startingLock)
                {
                    Monitor.Pulse(_startingLock);
                }

                Monitor.Wait(_runningLock);
            }

            while (true)
            {
                var state = this.State;

                switch (state)
                {
                    case LaborerState.Running:
                        try
                        {
                            var task = this.DoLabor(_signal.Token);

                            // do job
                            var vacationLength = await task;
                            vacationLength = TimeSpanExtensions.MinMax(
                                TimeQuantum,
                                VeryLongVacation,
                                vacationLength);

                            // go for vacation
                            await Task.Delay(vacationLength, _signal.Token);
                        }
                        catch (Exception ex)
                        {
                            if (_signal.IsCancellationRequested)
                            {
                                // cancellation was requested by 'Stop' or 'Pause'.

                                if (ex is TaskCanceledException)
                                {
                                    // 99.99% and more probability that these facts are connected - '_signal' was 'Cancel'-ed by 'Stop' or 'Pause' and 'await task' threw a 'TaskCanceledException' as it was supposed to.

                                    // do nothing - subsequent subroutine (Stop-related or Pause-related) will dispose '_signal'.
                                }
                                else
                                {
                                    // strange situation, should not ever happen. looks like one of the following happened:
                                    // 1. '_signal' was 'Cancel'-ed, but 'await Task' has thrown a non-'TaskCanceledException' exception in response, despite our hope. bad of him.
                                    // -or-
                                    // 2. 'await task' has thrown 'ex', and right after that '_signal' was 'Cancel'-ed.
                                    // -or-
                                    // 3. something else?

                                    // anyhow, let's log an exception and...
                                    this.Logger.LogWarning(ex, "Task failed.");

                                    // do nothing - subsequent subroutine (Stop-related or Pause-related) will dispose '_signal'.
                                }
                            }
                            else
                            {
                                this.Logger.LogWarning(ex, "Task failed.");
                                await this.SleepAfterError();
                            }
                        }

                        break;

                    case LaborerState.Stopping:
                        throw new NotImplementedException();
                        break;

                    case LaborerState.Pausing:
                        throw new NotImplementedException();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(); // todo
                }
            }
        }

        #endregion

        #region Public

        public TimeSpan ErrorTimeout
        {
            get
            {
                lock (_errorTimeoutLock)
                {
                    return _errorTimeout;
                }
            }
            set
            {
                lock (_errorTimeoutLock)
                {
                    if (value < TimeQuantum || value > VeryLongVacation)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value));
                    }

                    _errorTimeout = value;
                }
            }
        }

        #endregion
    }
}
