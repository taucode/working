﻿using Microsoft.Extensions.Logging;
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

        private readonly object _initLoopLock;
        private readonly object _loopIsInitedLock;

        private Task _loopTask;

        private CancellationTokenSource _controlSignal;
        private readonly AutoResetEvent _abortVacationSignal;

        #endregion

        #region Constructor

        protected LoopLaborerBase()
        {
            _initLoopLock = new object();
            _loopIsInitedLock = new object();

            _errorTimeout = DefaultErrorTimeout;
            _errorTimeoutLock = new object();

            _abortVacationSignal = new AutoResetEvent(false);
        }

        #endregion

        #region Abstract

        protected abstract Task<TimeSpan> DoLabor(CancellationToken cancellationToken);

        #endregion

        #region Overridden

        protected override void OnStarting() => this.InitLoop();

        protected override void OnStarted() => this.OnLoopInited();

        protected override void OnStopping() => this.StopLoop();

        protected override void OnStopped() => this.OnLoopStopped();

        protected override void OnPausing() => this.StopLoop();

        protected override void OnPaused() => this.OnLoopStopped();

        protected override void OnResuming() => this.InitLoop();

        protected override void OnResumed() => this.OnLoopInited();

        protected override void OnDisposed()
        {
            _abortVacationSignal.Dispose();
        }

        #endregion

        #region Private

        private void InitLoop()
        {
            lock (_initLoopLock)
            {
                _loopTask = Task.Run(this.LoopRoutine);
                Monitor.Wait(_initLoopLock);
            }
        }

        private void OnLoopInited()
        {
            _controlSignal = new CancellationTokenSource();

            lock (_loopIsInitedLock)
            {
                Monitor.Pulse(_loopIsInitedLock);
            }
        }

        private void StopLoop()
        {
            _controlSignal.Cancel();

            try
            {
                _loopTask.Wait();
            }
            catch (AggregateException)
            {
                // looks like task was canceled, that was the intent.
            }

            _loopTask.Dispose();
            _loopTask = null;
        }

        private void OnLoopStopped()
        {
            try
            {
                _controlSignal.Dispose();
            }
            catch
            {
                // dismiss
            }

            _controlSignal = null;

        }

        private async Task LoopRoutine()
        {
            lock (_loopIsInitedLock)
            {
                lock (_initLoopLock)
                {
                    Monitor.Pulse(_initLoopLock);
                }

                Monitor.Wait(_loopIsInitedLock);
            }

            var goOn = true;

            var handleArray = new[]
            {
                _controlSignal.Token.WaitHandle,
                _abortVacationSignal,
            };

            while (goOn)
            {
                var state = this.State;

                switch (state)
                {
                    case LaborerState.Running:
                        TimeSpan vacationLength;

                        try
                        {
                            vacationLength = await this.DoLabor(_controlSignal.Token);
                        }
                        catch (OperationCanceledException)
                        {
                            throw; // todo: consider checking _controlSignal.IsCancellationRequested
                        }
                        catch (Exception ex)
                        {
                            this.GetSafeLogger().LogError(ex, $"Exception occurred. Laborer name: '{this.Name}'.");
                            await Task.Delay(this.ErrorTimeout, _controlSignal.Token); // todo: can throw 'OperationCanceledException', ut it.

                            continue;
                        }

                        vacationLength = TimeSpanExtensions.MinMax(TimeQuantum, VeryLongVacation, vacationLength);
                        WaitHandle.WaitAny(handleArray, vacationLength);
                        _controlSignal.Token.ThrowIfCancellationRequested();

                        break;

                    case LaborerState.Stopping:
                    case LaborerState.Pausing:
                        goOn = false;
                        break;
                }
            }
        }

        #endregion

        #region Protected

        protected void AbortVacation()
        {
            _abortVacationSignal.Set(); // can throw 'ObjectDisposedException', so be it.
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