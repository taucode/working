using Serilog;
using System.Text;
using TauCode.Infrastructure.Logging;

namespace TauCode.Working;

public abstract class WorkerBase : IWorker
{
    #region Fields

    private long _stateValue;
    private long _isDisposedValue;

    private string? _name;

    private readonly object _controlLock;

    #endregion

    #region Constructor

    protected WorkerBase(ILogger? logger)
    {
        this.OriginalLogger = logger;

        this.ContextLogger = logger?
            .ForContext(new ObjectTagEnricher(this.GetTag));

        _controlLock = new object();

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

    private NotSupportedException CreatePausingNotSupportedException()
    {
        return new NotSupportedException("Pausing/resuming is not supported.");
    }

    #endregion

    #region Abstract

    public abstract bool IsPausingSupported { get; }

    protected abstract void OnBeforeStarting();
    protected abstract void OnAfterStarted();

    protected abstract void OnBeforeStopping();
    protected abstract void OnAfterStopped();

    protected abstract void OnBeforePausing();
    protected abstract void OnAfterPaused();

    protected abstract void OnBeforeResuming();
    protected abstract void OnAfterResumed();

    protected abstract void OnAfterDisposed();

    #endregion

    #region Protected

    protected virtual ObjectTag GetTag()
    {
        return new ObjectTag(this.GetType().Name!, this.Name);
    }

    protected ILogger? OriginalLogger { get; }

    protected ILogger? ContextLogger { get; set; }

    protected void Stop(bool throwOnDisposedOrWrongState)
    {
        lock (_controlLock)
        {
            if (this.GetIsDisposed())
            {
                if (throwOnDisposedOrWrongState)
                {
                    this.CheckNotDisposed(); // will throw
                }
                else
                {
                    return;
                }
            }

            var state = this.GetState();
            var isValidState =
                state == WorkerState.Running ||
                state == WorkerState.Paused;

            if (!isValidState)
            {
                if (throwOnDisposedOrWrongState)
                {
                    throw this.CreateInvalidOperationException(nameof(Stop), state);
                }
                else
                {
                    return;
                }
            }

            this.SetState(WorkerState.Stopping);

            this.ContextLogger?.Verbose(
                "Inside method '{0:l}'. About to call '{1:l}'.",
                nameof(Stop),
                nameof(OnBeforeStopping));
            this.OnBeforeStopping(); // todo: if 'OnStopping()' throws, worker must remain in prev state.

            this.SetState(WorkerState.Stopped);

            this.ContextLogger?.Verbose(
                "Inside method '{0:l}'. About to call '{1:l}'.",
                nameof(Stop),
                nameof(OnAfterStopped));
            this.OnAfterStopped(); // todo: log if throws

            this.ContextLogger?.Verbose(
                "Inside method '{0:l}'. Stopped successfully.",
                nameof(Stop));
        }
    }

    protected InvalidOperationException CreateInvalidOperationException(string requestedOperation, WorkerState actualState)
    {
        var sb = new StringBuilder();
        sb.Append($"Cannot perform operation '{requestedOperation}'. Worker state is '{actualState}'. Worker name is '{this.GetNameForDiagnostics()}'.");

        var message = sb.ToString();

        return new InvalidOperationException(message);
    }

    protected void ProhibitIfStateIs(string requestedOperation, params WorkerState[] prohibitedStates)
    {
        var actualState = this.GetState();
        if (prohibitedStates.Contains(actualState))
        {
            throw this.CreateInvalidOperationException(requestedOperation, actualState);
        }
    }

    protected void AllowIfStateIs(string requestedOperation, params WorkerState[] allowedStates)
    {
        var actualState = this.GetState();
        if (allowedStates.Contains(actualState))
        {
            // ok
        }
        else
        {
            throw this.CreateInvalidOperationException(requestedOperation, actualState);
        }
    }

    protected string GetNameForDiagnostics() => _name ?? this.GetType().FullName!;

    protected void CheckNotDisposed()
    {
        if (this.GetIsDisposed())
        {
            throw new ObjectDisposedException(this.GetNameForDiagnostics());
        }
    }

    #endregion

    #region Internal

    //internal string GetWorkerCaptionForLog()
    //{
    //    // todo: cache when 'Name' changed.

    //    var sb = new StringBuilder();
    //    sb.Append(" (Worker: '");
    //    sb.Append(this.GetNameForDiagnostics());
    //    sb.Append("')");

    //    return sb.ToString();
    //}

    #endregion

    #region IWorker Members

    public string? Name
    {
        get => _name;
        set
        {
            this.CheckNotDisposed();

            _name = value;
        }
    }

    public WorkerState State => this.GetState();

    public void Start()
    {
        lock (_controlLock)
        {
            this.CheckNotDisposed();

            var state = this.GetState();
            this.AllowIfStateIs(nameof(Start), WorkerState.Stopped);

            this.SetState(WorkerState.Starting);

            try
            {
                this.ContextLogger?.Verbose(
                    "Inside method '{0:l}'. About to call '{1:l}'.",
                    nameof(Start),
                    nameof(OnBeforeStarting));

                this.OnBeforeStarting(); // todo: if thrown, worker remains in 'Starting' state! critical error!
            }
            catch (Exception ex)
            {
                this.ContextLogger?.Error(
                    ex,
                    "Inside method '{0:l}'. '{1:l}' has thrown an exception, so worker will remain in the state '{2}'.",
                    nameof(Start),
                    nameof(OnBeforeStarting),
                    WorkerState.Stopped);

                this.SetState(WorkerState.Stopped);

                throw;
            }

            this.SetState(WorkerState.Running);

            this.ContextLogger?.Verbose(
                "Inside method '{0:l}'. About to call '{1:l}'.",
                nameof(Start),
                nameof(OnAfterStarted));

            this.OnAfterStarted();
        }
    }

    public void Stop() => this.Stop(true);

    public void Pause()
    {
        if (!IsPausingSupported)
        {
            throw this.CreatePausingNotSupportedException();
        }

        lock (_controlLock)
        {
            this.CheckNotDisposed();
            this.AllowIfStateIs(nameof(Pause), WorkerState.Running);

            this.SetState(WorkerState.Pausing);

            this.ContextLogger?.Verbose(nameof(OnBeforePausing));
            this.OnBeforePausing(); // todo: if 'OnPausing()' throws, then must remain in 'Running' state

            this.SetState(WorkerState.Paused);

            this.ContextLogger?.Verbose(nameof(OnAfterPaused));
            this.OnAfterPaused(); // todo: log if thrown
        }
    }

    public void Resume()
    {
        if (!IsPausingSupported)
        {
            throw this.CreatePausingNotSupportedException();
        }

        lock (_controlLock)
        {
            this.CheckNotDisposed();
            this.AllowIfStateIs(nameof(Resume), WorkerState.Paused);

            // todo clean
            //var state = this.GetState();
            //if (state != WorkerState.Paused)
            //{
            //    throw this.CreateInvalidOperationException(nameof(Resume), state);
            //}

            this.SetState(WorkerState.Resuming);

            this.ContextLogger?.Verbose(nameof(OnBeforeResuming));
            this.OnBeforeResuming(); // todo: if 'OnResuming()' throws, then must remain in 'Running' state

            this.SetState(WorkerState.Running);

            this.ContextLogger?.Verbose(nameof(OnAfterResumed));
            this.OnAfterResumed(); // todo: log if throws
        }
    }

    public bool IsDisposed => this.GetIsDisposed();

    #endregion

    #region IDisposable Members

    public void Dispose()
    {
        lock (_controlLock)
        {
            if (this.GetIsDisposed())
            {
                return; // won't dispose twice
            }

            this.Stop(false);

            this.SetIsDisposed(true);

            this.ContextLogger?.Verbose(
                "Inside method '{0:l}'. About to call '{1:l}'.",
                nameof(Dispose),
                nameof(OnAfterDisposed));
            this.OnAfterDisposed();
        }
    }

    #endregion
}