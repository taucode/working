using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TauCode.Extensions;

namespace TauCode.Working.Labor
{
    // todo clean
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

        private readonly object _pausingLock;
        private readonly object _pausedLock;

        private readonly object _resumingLock;
        private readonly object _resumedLock;

        private Task _loopTask;

        private CancellationTokenSource _controlSignal;

        private CancellationTokenSource _abortVacationSignal;
        private readonly object _vacationLock;

        #endregion

        #region Constructor

        protected LoopLaborerBase()
        {
            _startingLock = new object();
            _runningLock = new object();

            _pausingLock = new object();
            _pausedLock = new object();

            _resumingLock = new object();
            _resumedLock = new object();

            _errorTimeout = DefaultErrorTimeout;
            _errorTimeoutLock = new object();

            _vacationLock = new object();
        }

        #endregion

        #region Abstract

        protected abstract Task<TimeSpan> DoLabor(CancellationToken cancellationToken);

        #endregion

        #region Overridden

        protected override void OnStarting()
        {
            lock (_startingLock)
            {
                _loopTask = Task.Run(this.LoopRoutine);
                Monitor.Wait(_startingLock);
            }
        }

        protected override void OnStarted()
        {
            _controlSignal = new CancellationTokenSource();
            _abortVacationSignal = CancellationTokenSource.CreateLinkedTokenSource(_controlSignal.Token);

            lock (_runningLock)
            {
                Monitor.Pulse(_runningLock);
            }
        }

        protected override void OnStopping()
        {
            _controlSignal.Cancel();
            _loopTask.Wait();
            _loopTask.Dispose();
            _loopTask = null;
        }

        protected override void OnStopped()
        {
            try
            {
                _abortVacationSignal.Dispose();
            }
            catch
            {
                // dismiss
            }

            try
            {
                _controlSignal.Dispose();
            }
            catch
            {
                // dismiss
            }

            _abortVacationSignal = null;
            _controlSignal = null;
        }

        protected override void OnPausing()
        {
            lock (_pausingLock)
            {
                _controlSignal.Cancel();
                Monitor.Wait(_pausingLock);
            }
        }

        protected override void OnPaused()
        {
            lock (_pausedLock)
            {
                Monitor.Pulse(_pausedLock);
            }
        }

        protected override void OnResuming()
        {
            lock (_resumingLock)
            {
                _controlSignal.Cancel();
                Monitor.Wait(_resumingLock);
            }
        }

        protected override void OnResumed()
        {
            lock (_resumedLock)
            {
                Monitor.Pulse(_resumedLock);
            }
        }

        protected override void OnDisposed()
        {
            // idle
        }

        #endregion

        #region Private

        private async Task SleepAfterError()
        {
            try
            {
                await Task.Delay(this.ErrorTimeout, _controlSignal.Token);
            }
            catch (TaskCanceledException)
            {
                // dismiss.
            }
        }

        private async Task<bool> PauseRoutine()
        {
            lock (_pausingLock)
            {
                _abortVacationSignal.Dispose();
                _controlSignal.Dispose();

                _controlSignal = new CancellationTokenSource();
                _abortVacationSignal = CancellationTokenSource.CreateLinkedTokenSource(_controlSignal.Token);

                lock (_pausedLock)
                {
                    Monitor.Pulse(_pausingLock);
                    Monitor.Wait(_pausedLock);
                }
            }

            try
            {
                await Task.Delay(-1, _controlSignal.Token);
            }
            catch (OperationCanceledException)
            {
                // dismiss
            }

            var state = this.State;

            if (state == LaborerState.Stopping)
            {
                return false;
            }

            // state MUST be 'Resuming' here.
            lock (_resumingLock)
            {
                _abortVacationSignal.Dispose();
                _controlSignal.Dispose();

                _controlSignal = new CancellationTokenSource();
                _abortVacationSignal = CancellationTokenSource.CreateLinkedTokenSource(_controlSignal.Token);

                lock (_resumedLock)
                {
                    Monitor.Pulse(_resumingLock);
                    Monitor.Wait(_resumedLock);
                }
            }

            return true;
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

            var goOn = true;

            while (goOn)
            {
                var state = this.State;

                lock (_vacationLock)
                {
                    if (_abortVacationSignal.IsCancellationRequested)
                    {
                        _abortVacationSignal.Dispose();
                        _abortVacationSignal = CancellationTokenSource.CreateLinkedTokenSource(_controlSignal.Token);
                    }
                }

                switch (state)
                {
                    case LaborerState.Running:
                        TimeSpan vacationLength;

                        try
                        {
                            vacationLength = await this.DoLabor(_controlSignal.Token); // [1]
                        }
                        catch (OperationCanceledException)
                        {
                            if (_controlSignal.IsCancellationRequested)
                            {
                                // todo: ut this execution branch
                                // that's predictable
                                continue;
                            }
                            else
                            {
                                // todo: ut this execution branch
                                // strange. looks like [1] has thrown the 'OperationCanceledException' without '_controlSignal' being canceled.
                                this.GetSafeLogger().LogWarning($"'{nameof(OperationCanceledException)}' was thrown without canceling task. Laborer name: '{this.Name}'.");
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            if (_controlSignal.IsCancellationRequested)
                            {
                                // todo: ut this execution branch
                                // todo: comment
                                continue;
                            }
                            else
                            {
                                // todo: ut this execution branch
                                // error occurred. let's log it, sleep and go on.
                                this.GetSafeLogger().LogError(ex, $"Exception occurred. Laborer name: '{this.Name}'");
                                await this.SleepAfterError();
                                continue;
                            }
                        }

                        vacationLength = TimeSpanExtensions.MinMax(TimeQuantum, VeryLongVacation, vacationLength);

                        try
                        {
                            await Task.Delay(vacationLength, _abortVacationSignal.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            // either '_controlSignal' or '_abortVacationSignal' was canceled.
                        }

                        break;

                    case LaborerState.Stopping:
                        goOn = false;
                        break;

                    case LaborerState.Pausing:
                        var wasResumed = await this.PauseRoutine();

                        if (wasResumed)
                        {
                            // will continue the loop
                        }
                        else
                        {
                            // was stopped

                            throw new NotImplementedException();
                        }

                        break;

                    default:
                        throw new ArgumentOutOfRangeException(); // todo
                }
            }
        }

        //private async Task LoopRoutine_old()
        //{
        //    lock (_runningLock)
        //    {
        //        lock (_startingLock)
        //        {
        //            Monitor.Pulse(_startingLock);
        //        }

        //        Monitor.Wait(_runningLock);
        //    }

        //    while (true)
        //    {
        //        var state = this.State;

        //        switch (state)
        //        {
        //            case LaborerState.Running:
        //                try
        //                {
        //                    var task = this.DoLabor(_signal.Token); // todo: _laborSignal, _vacationSignal. Think of QueueLaborer.

        //                    // do job
        //                    var vacationLength = await task;
        //                    vacationLength = TimeSpanExtensions.MinMax(
        //                        TimeQuantum,
        //                        VeryLongVacation,
        //                        vacationLength);

        //                    // go for vacation
        //                    await Task.Delay(vacationLength, _signal.Token);
        //                }
        //                catch (Exception ex)
        //                {
        //                    if (_signal.IsCancellationRequested)
        //                    {
        //                        // cancellation was requested by 'Stop' or 'Pause'.

        //                        if (ex is TaskCanceledException)
        //                        {
        //                            // 99.99% and more probability that these facts are connected - '_signal' was 'Cancel'-ed by 'Stop' or 'Pause' and 'await task' threw a 'TaskCanceledException' as it was supposed to.

        //                            // do nothing - subsequent subroutine (Stop-related or Pause-related) will dispose '_signal'.
        //                        }
        //                        else
        //                        {
        //                            // strange situation, should not ever happen. looks like one of the following happened:
        //                            // 1. '_signal' was 'Cancel'-ed, but 'await Task' has thrown a non-'TaskCanceledException' exception in response, despite our hope. bad of him.
        //                            // -or-
        //                            // 2. 'await task' has thrown 'ex', and right after that '_signal' was 'Cancel'-ed.
        //                            // -or-
        //                            // 3. something else?

        //                            // anyhow, let's log an exception and...
        //                            this.Logger.LogWarning(ex, "Task failed.");

        //                            // do nothing - subsequent subroutine (Stop-related or Pause-related) will dispose '_signal'.
        //                        }
        //                    }
        //                    else
        //                    {
        //                        this.Logger.LogWarning(ex, "Task failed.");
        //                        await this.SleepAfterError();
        //                    }
        //                }

        //                break;

        //            case LaborerState.Stopping:
        //                throw new NotImplementedException();
        //                break;

        //            case LaborerState.Pausing:
        //                throw new NotImplementedException();
        //                break;

        //            default:
        //                throw new ArgumentOutOfRangeException(); // todo
        //        }
        //    }
        //}

        #endregion

        #region Protected

        protected void AbortVacation()
        {
            lock (_vacationLock)
            {
                try
                {
                    _abortVacationSignal.Cancel();
                }
                catch
                {
                    // dismiss
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
