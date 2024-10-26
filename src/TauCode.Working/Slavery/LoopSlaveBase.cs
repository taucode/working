using Serilog;
using TauCode.Extensions;

namespace TauCode.Working.Slavery;

public abstract class LoopSlaveBase : SlaveBase
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
    private readonly object _loopIsInitializedLock;

    private Task? _loopTask;

    private CancellationTokenSource? _controlSignal;
    private readonly AutoResetEvent _abortVacationSignal;

    #endregion

    #region Constructor

    protected LoopSlaveBase(ILogger? logger)
        : base(logger)
    {
        _initLoopLock = new object();
        _loopIsInitializedLock = new object();

        _errorTimeout = DefaultErrorTimeout;
        _errorTimeoutLock = new object();

        _abortVacationSignal = new AutoResetEvent(false);
    }

    #endregion

    #region Abstract

    protected abstract Task<TimeSpan> DoWork(CancellationToken cancellationToken);

    #endregion

    #region Overridden

    protected override void OnBeforeStarting() => InitLoop();

    protected override void OnAfterStarted() => OnLoopInitialized();

    protected override void OnBeforeStopping() => StopLoop();

    protected override void OnAfterStopped() => OnLoopStopped();

    protected override void OnBeforePausing() => StopLoop();

    protected override void OnAfterPaused() => OnLoopStopped();

    protected override void OnBeforeResuming() => InitLoop();

    protected override void OnAfterResumed() => OnLoopInitialized();

    protected override void OnAfterDisposed()
    {
        _abortVacationSignal.Dispose();
    }

    #endregion

    #region Private

    private void InitLoop()
    {
        lock (_initLoopLock)
        {
            _loopTask = Task.Run(LoopRoutine);
            Monitor.Wait(_initLoopLock);
        }
    }

    private void OnLoopInitialized()
    {
        _controlSignal = new CancellationTokenSource();

        lock (_loopIsInitializedLock)
        {
            Monitor.Pulse(_loopIsInitializedLock);
        }
    }

    private void StopLoop()
    {
        _controlSignal?.Cancel();

        try
        {
            _loopTask?.Wait();
        }
        catch (AggregateException)
        {
            // looks like task was canceled, that was the intent.
        }

        _loopTask?.Dispose();
        _loopTask = null;
    }

    private void OnLoopStopped()
    {
        try
        {
            _controlSignal?.Dispose();
        }
        catch
        {
            // dismiss
        }

        _controlSignal = null;

    }

    private async Task LoopRoutine()
    {
        lock (_loopIsInitializedLock)
        {
            lock (_initLoopLock)
            {
                Monitor.Pulse(_initLoopLock);
            }

            Monitor.Wait(_loopIsInitializedLock);
        }

        var goOn = true;

        var handleArray = new[]
        {
            (_controlSignal ?? throw new Exception("Internal error.")) .Token.WaitHandle,
            _abortVacationSignal,
        };

        while (goOn)
        {
            var state = State;

            switch (state)
            {
                case SlaveState.Running:
                    var vacationLength = TimeSpan.Zero;

                    Exception? thrownException;
                    string? messageForThrownException;

                    try
                    {
                        vacationLength = await DoWork(_controlSignal.Token);

                        // success.
                        thrownException = null;
                        messageForThrownException = null;
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (_controlSignal.IsCancellationRequested)
                        {
                            throw; // control signal was requested
                        }

                        // 'DoWork' has thrown a 'OperationCanceledException' without prompted to. Bad of him!
                        thrownException = ex;
                        messageForThrownException =
                            $"Unexpected '{nameof(OperationCanceledException)}' in '{nameof(LoopRoutine)}'.";
                    }
                    catch (Exception ex)
                    {
                        thrownException = ex;
                        messageForThrownException = $"Exception occurred in '{nameof(LoopRoutine)}'.";
                    }

                    if (thrownException != null)
                    {
                        ContextLogger?.Error(thrownException, messageForThrownException!);
                        await Task.Delay(ErrorTimeout, _controlSignal.Token); // todo: can throw 'OperationCanceledException', ut it.
                        continue;
                    }

                    vacationLength = TimeSpanExtensions.MinMax(TimeQuantum, VeryLongVacation, vacationLength);
                    WaitHandle.WaitAny(handleArray, vacationLength);
                    _controlSignal.Token.ThrowIfCancellationRequested();

                    break;

                case SlaveState.Stopping:
                case SlaveState.Pausing:
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